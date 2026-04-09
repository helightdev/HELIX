using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HELIX.Widgets.Diagnostics.Formatting {
    public sealed class TextTreeRenderer {
        private readonly int _maxDescendantsTruncatableNode;
        private readonly DiagnosticLevel _minLevel;
        private readonly int _wrapWidth;
        private readonly int _wrapWidthProperties;

        public TextTreeRenderer(
            DiagnosticLevel minLevel = DiagnosticLevel.Debug,
            int wrapWidth = 100,
            int wrapWidthProperties = 65,
            int maxDescendantsTruncatableNode = -1
        ) {
            _minLevel = minLevel;
            _wrapWidth = wrapWidth;
            _wrapWidthProperties = wrapWidthProperties;
            _maxDescendantsTruncatableNode = maxDescendantsTruncatableNode;
        }

        public string Render(
            DiagnosticsNode node,
            string prefixLineOne = "",
            string prefixOtherLines = null,
            TextTreeConfiguration parentConfiguration = null
        ) {
            return DebugRender(node, prefixLineOne, prefixOtherLines, parentConfiguration);
        }

        private TextTreeConfiguration ChildTextConfiguration(DiagnosticsNode child, TextTreeConfiguration textStyle) {
            var childStyle = child.Style;
            return DiagnosticsNode.IsSingleLine(childStyle) || childStyle == DiagnosticsTreeStyle.ErrorProperty
                ? textStyle
                : child.GetTextTreeConfiguration();
        }

        private string DebugRender(
            DiagnosticsNode node,
            string prefixLineOne,
            string prefixOtherLines,
            TextTreeConfiguration parentConfiguration
        ) {
            var isSingleLine = DiagnosticsNode.IsSingleLine(node.Style) &&
                               (parentConfiguration == null || !parentConfiguration.LineBreakProperties);

            prefixOtherLines ??= prefixLineOne;
            if (!string.IsNullOrEmpty(node.LinePrefix)) {
                prefixLineOne += node.LinePrefix;
                prefixOtherLines += node.LinePrefix;
            }

            var config = node.GetTextTreeConfiguration();
            if (prefixOtherLines.Length == 0) prefixOtherLines += config.PrefixOtherLinesRootNode;

            if (node.Style == DiagnosticsTreeStyle.TruncateChildren) {
                var descendants = new List<string>();
                const int maxDepth = 5;
                const int maxLines = 25;
                var depth = 0;
                var lines = 0;

                void Visitor(DiagnosticsNode current) {
                    foreach (var child in current.GetChildren()) {
                        if (lines < maxLines) {
                            depth++;
                            descendants.Add(prefixOtherLines + new string(' ', depth * 2) + child);
                            if (depth < maxDepth) Visitor(child);
                            depth--;
                        } else if (lines == maxLines) {
                            descendants.Add(
                                prefixOtherLines + "  ...(descendants list truncated after " + lines + " lines)"
                            );
                        }

                        lines++;
                    }
                }

                Visitor(node);
                var info = new StringBuilder(prefixLineOne);
                if (lines > 1) {
                    info.AppendLine(
                        "This " + node.Name + " had the following descendants (showing up to depth " + maxDepth + "):"
                    );
                } else if (descendants.Count == 1) info.AppendLine("This " + node.Name + " had the following child:");
                else info.AppendLine("This " + node.Name + " has no descendants.");

                info.Append(string.Join("\n", descendants));
                return info.ToString();
            }

            var builder = new PrefixedStringBuilder(
                prefixLineOne,
                prefixOtherLines,
                Math.Max(_wrapWidth, prefixOtherLines.Length + _wrapWidthProperties)
            );

            var children = node.GetChildren();
            var description = node.ToDescription(parentConfiguration);

            if (config.BeforeName.Length > 0) builder.Write(config.BeforeName);

            var wrapName = !isSingleLine && node.AllowNameWrap;
            var wrapDescription = !isSingleLine && node.AllowWrap;
            var uppercaseTitle = node.Style == DiagnosticsTreeStyle.Error;
            var name = node.Name;
            if (uppercaseTitle && name != null) name = name.ToUpperInvariant();

            if (string.IsNullOrEmpty(description)) {
                if (node.ShowName && !string.IsNullOrEmpty(name)) builder.Write(name, wrapName);
            } else {
                var includeName = false;
                if (!string.IsNullOrEmpty(name) && node.ShowName) {
                    includeName = true;
                    builder.Write(name, wrapName);
                    if (node.ShowSeparator) builder.Write(config.AfterName, wrapName);

                    builder.Write(config.IsNameOnOwnLine || description.Contains("\n") ? "\n" : " ", wrapName);
                }

                if (!isSingleLine && builder.RequiresMultipleLines && !builder.IsCurrentLineEmpty) builder.Write("\n");

                if (includeName) {
                    builder.IncrementPrefixOtherLines(
                        children.Count == 0 ? config.PropertyPrefixNoChildren : config.PropertyPrefixIfChildren,
                        true
                    );
                }

                if (uppercaseTitle) description = description.ToUpperInvariant();

                builder.Write(description.TrimEnd(), wrapDescription);

                if (!includeName) {
                    builder.IncrementPrefixOtherLines(
                        children.Count == 0 ? config.PropertyPrefixNoChildren : config.PropertyPrefixIfChildren,
                        false
                    );
                }
            }

            if (config.SuffixLineOne.Length > 0)
                builder.WriteStretched(config.SuffixLineOne, builder.WrapWidth ?? _wrapWidth);

            var propertiesEnumerable =
                node.GetProperties().Where(n => !n.IsFiltered(_minLevel));
            List<DiagnosticsNode> properties;

            if (_maxDescendantsTruncatableNode >= 0 && node.AllowTruncate) {
                properties = propertiesEnumerable.Take(_maxDescendantsTruncatableNode).ToList();
                if (properties.Count >= _maxDescendantsTruncatableNode)
                    properties.Add(DiagnosticsNode.Message("..."));

                if (_maxDescendantsTruncatableNode < children.Count) {
                    children = children.Take(_maxDescendantsTruncatableNode).ToList();
                    children.Add(DiagnosticsNode.Message("..."));
                }
            } else properties = propertiesEnumerable.ToList();

            if ((properties.Count > 0 || children.Count > 0 || node.EmptyBodyDescription != null) &&
                (node.ShowSeparator || !string.IsNullOrEmpty(description)))
                builder.Write(config.AfterDescriptionIfBody);

            if (config.LineBreakProperties) builder.Write(config.LineBreak);

            if (properties.Count > 0) builder.Write(config.BeforeProperties);

            builder.IncrementPrefixOtherLines(config.BodyIndent, false);

            if (node.EmptyBodyDescription != null && properties.Count == 0 && children.Count == 0 &&
                prefixLineOne.Length > 0) {
                builder.Write(node.EmptyBodyDescription);
                if (config.LineBreakProperties) builder.Write(config.LineBreak);
            }

            for (var i = 0; i < properties.Count; i++) {
                var property = properties[i];
                if (i > 0) builder.Write(config.PropertySeparator);

                var propertyStyle = property.GetTextTreeConfiguration();
                if (DiagnosticsNode.IsSingleLine(property.Style)) {
                    var propertyRender = Render(
                        property,
                        propertyStyle.PrefixLineOne,
                        propertyStyle.ChildLinkSpace + propertyStyle.PrefixOtherLines,
                        config
                    );

                    var propertyLines = propertyRender.Split('\n');
                    if (propertyLines.Length == 1 && !config.LineBreakProperties) builder.Write(propertyLines[0]);
                    else {
                        builder.Write(propertyRender);
                        if (!propertyRender.EndsWith("\n", StringComparison.Ordinal)) builder.Write("\n");
                    }
                } else {
                    var propertyRender = Render(
                        property,
                        builder.PrefixOtherLines + propertyStyle.PrefixLineOne,
                        builder.PrefixOtherLines + propertyStyle.ChildLinkSpace + propertyStyle.PrefixOtherLines,
                        config
                    );
                    builder.WriteRawLines(propertyRender);
                }
            }

            if (properties.Count > 0) builder.Write(config.AfterProperties);

            builder.Write(config.MandatoryAfterProperties);

            if (!config.LineBreakProperties) builder.Write(config.LineBreak);

            var prefixChildren = config.BodyIndent;
            var prefixChildrenRaw = prefixOtherLines + prefixChildren;

            if (children.Count == 0 &&
                config.AddBlankLineIfNoChildren &&
                builder.RequiresMultipleLines &&
                builder.PrefixOtherLines.TrimEnd().Length > 0) builder.Write(config.LineBreak);

            if (children.Count > 0 && config.ShowChildren) {
                if (config.IsBlankLineBetweenPropertiesAndChildren &&
                    properties.Count > 0 &&
                    children[0].GetTextTreeConfiguration().IsBlankLineBetweenPropertiesAndChildren)
                    builder.Write(config.LineBreak);

                builder.PrefixOtherLines = prefixOtherLines;

                for (var i = 0; i < children.Count; i++) {
                    var child = children[i];
                    var childConfig = ChildTextConfiguration(child, config);

                    if (i == children.Count - 1) {
                        var lastChildPrefixLineOne = prefixChildrenRaw + childConfig.PrefixLastChildLineOne;
                        var childPrefixOtherLines = prefixChildrenRaw + childConfig.ChildLinkSpace +
                                                    childConfig.PrefixOtherLines;
                        builder.WriteRawLines(Render(child, lastChildPrefixLineOne, childPrefixOtherLines, config));

                        if (childConfig.Footer.Length > 0) {
                            builder.PrefixOtherLines = prefixChildrenRaw;
                            builder.Write(childConfig.ChildLinkSpace + childConfig.Footer);
                            if (childConfig.MandatoryFooter.Length > 0) {
                                builder.WriteStretched(
                                    childConfig.MandatoryFooter,
                                    Math.Max(
                                        builder.WrapWidth ?? _wrapWidth,
                                        _wrapWidthProperties + childPrefixOtherLines.Length
                                    )
                                );
                            }

                            builder.Write(config.LineBreak);
                        }
                    } else {
                        var nextChildStyle = ChildTextConfiguration(children[i + 1], config);
                        var childPrefixLineOne = prefixChildrenRaw + childConfig.PrefixLineOne;
                        var childPrefixOtherLines = prefixChildrenRaw + nextChildStyle.LinkCharacter +
                                                    childConfig.PrefixOtherLines;
                        builder.WriteRawLines(Render(child, childPrefixLineOne, childPrefixOtherLines, config));

                        if (childConfig.Footer.Length > 0) {
                            builder.PrefixOtherLines = prefixChildrenRaw;
                            builder.Write(childConfig.LinkCharacter + childConfig.Footer);
                            if (childConfig.MandatoryFooter.Length > 0) {
                                builder.WriteStretched(
                                    childConfig.MandatoryFooter,
                                    Math.Max(
                                        builder.WrapWidth ?? _wrapWidth,
                                        _wrapWidthProperties + childPrefixOtherLines.Length
                                    )
                                );
                            }

                            builder.Write(config.LineBreak);
                        }
                    }
                }
            }

            if (parentConfiguration == null && config.MandatoryFooter.Length > 0) {
                builder.WriteStretched(config.MandatoryFooter, builder.WrapWidth ?? _wrapWidth);
                builder.Write(config.LineBreak);
            }

            return builder.Build();
        }
    }
}
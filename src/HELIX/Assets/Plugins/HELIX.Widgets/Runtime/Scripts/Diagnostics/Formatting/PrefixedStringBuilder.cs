using System;
using System.Collections.Generic;
using System.Text;

namespace HELIX.Widgets.Diagnostics.Formatting {
    internal sealed class PrefixedStringBuilder {
        private readonly StringBuilder _buffer = new();
        private readonly StringBuilder _currentLine = new();
        private readonly List<int> _wrappableRanges = new();
        private string _nextPrefixOtherLines;
        private int _numLines;
        private string _prefixOtherLines;

        public PrefixedStringBuilder(string prefixLineOne, string prefixOtherLines, int? wrapWidth) {
            PrefixLineOne = prefixLineOne ?? string.Empty;
            _prefixOtherLines = prefixOtherLines ?? string.Empty;
            WrapWidth = wrapWidth;
        }

        public string PrefixLineOne { get; }
        public int? WrapWidth { get; }

        public string PrefixOtherLines {
            get => _nextPrefixOtherLines ?? _prefixOtherLines;
            set {
                _prefixOtherLines = value ?? string.Empty;
                _nextPrefixOtherLines = null;
            }
        }

        public bool IsCurrentLineEmpty => _currentLine.Length == 0;

        public bool RequiresMultipleLines {
            get {
                if (!WrapWidth.HasValue) return _numLines > 1 || (_numLines == 1 && _currentLine.Length > 0);

                return _numLines > 1 ||
                       (_numLines == 1 && _currentLine.Length > 0) ||
                       _currentLine.Length + GetCurrentPrefix(true).Length > WrapWidth.Value;
            }
        }

        public void IncrementPrefixOtherLines(string suffix, bool updateCurrentLine) {
            suffix = suffix ?? string.Empty;
            if (_currentLine.Length == 0 || updateCurrentLine) {
                _prefixOtherLines = PrefixOtherLines + suffix;
                _nextPrefixOtherLines = null;
            } else _nextPrefixOtherLines = PrefixOtherLines + suffix;
        }

        public void Write(string s, bool allowWrap = false) {
            if (string.IsNullOrEmpty(s)) return;

            var lines = s.Split('\n');
            for (var i = 0; i < lines.Length; i++) {
                if (i > 0) {
                    FinalizeLine(true);
                    UpdatePrefix();
                }

                var line = lines[i];
                if (line.Length == 0) continue;

                if (allowWrap && WrapWidth.HasValue) {
                    var wrapStart = _currentLine.Length;
                    var wrapEnd = wrapStart + line.Length;
                    if (_wrappableRanges.Count > 0 && _wrappableRanges[^1] == wrapStart) _wrappableRanges[^1] = wrapEnd;
                    else {
                        _wrappableRanges.Add(wrapStart);
                        _wrappableRanges.Add(wrapEnd);
                    }
                }

                _currentLine.Append(line);
            }
        }

        public void WriteRawLines(string lines) {
            if (string.IsNullOrEmpty(lines)) return;

            if (_currentLine.Length > 0) FinalizeLine(true);

            _buffer.Append(lines);
            if (!lines.EndsWith("\n", StringComparison.Ordinal)) _buffer.Append('\n');

            _numLines++;
            UpdatePrefix();
        }

        public void WriteStretched(string text, int targetLineLength) {
            Write(text);
            var currentLineLength = _currentLine.Length + GetCurrentPrefix(_buffer.Length == 0).Length;
            var targetLength = targetLineLength - currentLineLength;
            if (targetLength > 0 && !string.IsNullOrEmpty(text)) {
                var last = text[^1];
                _currentLine.Append(new string(last, targetLength));
            }

            _wrappableRanges.Clear();
        }

        public string Build() {
            if (_currentLine.Length > 0) FinalizeLine(false);
            return _buffer.ToString();
        }

        private void UpdatePrefix() {
            if (_nextPrefixOtherLines != null) {
                _prefixOtherLines = _nextPrefixOtherLines;
                _nextPrefixOtherLines = null;
            }
        }

        private void FinalizeLine(bool addTrailingLineBreak) {
            var firstLine = _buffer.Length == 0;
            var text = _currentLine.ToString();
            _currentLine.Clear();

            if (_wrappableRanges.Count == 0 || !WrapWidth.HasValue) {
                WriteLine(text, addTrailingLineBreak, firstLine);
                return;
            }

            var lines = WordWrapLine(
                text,
                _wrappableRanges,
                WrapWidth.Value,
                firstLine ? PrefixLineOne.Length : _prefixOtherLines.Length,
                _prefixOtherLines.Length
            );

            for (var i = 0; i < lines.Count; i++) {
                var includeBreak = addTrailingLineBreak || i < lines.Count - 1;
                WriteLine(lines[i], includeBreak, firstLine && i == 0);
            }

            _wrappableRanges.Clear();
        }

        private void WriteLine(string line, bool includeLineBreak, bool firstLine) {
            var fullLine = GetCurrentPrefix(firstLine) + line;
            _buffer.Append(fullLine.TrimEnd());
            if (includeLineBreak) _buffer.Append('\n');
            _numLines++;
        }

        private string GetCurrentPrefix(bool firstLine) {
            return firstLine ? PrefixLineOne : _prefixOtherLines;
        }

        private static List<string> WordWrapLine(
            string message,
            List<int> wrapRanges,
            int width,
            int startOffset,
            int otherLineOffset
        ) {
            if (message.Length + startOffset < width) return new List<string> { message };

            var wrapped = new List<string>();
            var startForLengthCalculations = -startOffset;
            var index = 0;
            var mode = WordWrapParseMode.InSpace;
            var lastWordStart = 0;
            int? lastWordEnd = null;
            var start = 0;
            var currentChunk = 0;

            bool NoWrap(int i) {
                while (true) {
                    if (currentChunk >= wrapRanges.Count) return true;

                    if (i < wrapRanges[currentChunk + 1]) break;

                    currentChunk += 2;
                }

                return i < wrapRanges[currentChunk];
            }

            while (true) {
                switch (mode) {
                    case WordWrapParseMode.InSpace:
                        while (index < message.Length && message[index] == ' ') index++;
                        lastWordStart = index;
                        mode = WordWrapParseMode.InWord;
                        break;

                    case WordWrapParseMode.InWord:
                        while (index < message.Length && (message[index] != ' ' || NoWrap(index))) index++;
                        mode = WordWrapParseMode.AtBreak;
                        break;

                    case WordWrapParseMode.AtBreak:
                        if (index - startForLengthCalculations > width || index == message.Length) {
                            if (index - startForLengthCalculations <= width || !lastWordEnd.HasValue)
                                lastWordEnd = index;

                            var line = message.Substring(start, lastWordEnd.Value - start);
                            wrapped.Add(line);

                            if (lastWordEnd.Value >= message.Length) return wrapped;

                            if (lastWordEnd.Value == index) {
                                while (index < message.Length && message[index] == ' ') index++;
                                start = index;
                                mode = WordWrapParseMode.InWord;
                            } else {
                                start = lastWordStart;
                                mode = WordWrapParseMode.AtBreak;
                            }

                            startForLengthCalculations = start - otherLineOffset;
                            lastWordEnd = null;
                        } else {
                            lastWordEnd = index;
                            mode = WordWrapParseMode.InSpace;
                        }

                        break;
                }
            }
        }
    }
}
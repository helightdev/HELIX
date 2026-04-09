using System;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;

namespace HELIX.Widgets.Elements {
    public abstract class BuildingWidgetBaseElement<T> : SuperSingleChildWidgetBaseElement<T> where T : Widget {
        public Widget LastBuildResult { get; protected set; }
        public bool IsBuilding { get; protected set; }

        public virtual void BeforeBuild(T previous, T widget) { }

        public virtual void AfterBuild(T previous, T widget) { }

        public virtual Widget Build(IBuildable buildable, T previous, T widget) {
            return buildable.Build(this);
        }

        protected override Widget GetChildFromWidget(T previous, T widget) {
            IsBuilding = true;
            BuildContext.Current = this;
            try {
                BeforeBuild(previous, widget);
                var buildable = GetBuildableForWidget(previous, widget);
                if (buildable == null) return null;
                var built = Build(buildable, previous, widget);
                AfterBuild(previous, widget);
                LastBuildResult = built;
                return built;
            } catch (HelixDiagnosticException) { throw; } catch (Exception ex) {
                throw HelixDiagnostics.Build(
                    "An error occurred while building a widget.",
                    collector => { collector.OffendingWidget(widget).OwnerChain(this); },
                    ex
                );
            } finally {
                IsBuilding = false;
                BuildContext.Current = null;
            }
        }

        protected abstract IBuildable GetBuildableForWidget(T previous, T widget);
    }
}
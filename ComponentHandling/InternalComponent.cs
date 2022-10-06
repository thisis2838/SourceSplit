using LiveSplit.Model;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    public class InternalComponent : InfoTextComponent
    {
        public static List<InternalComponent> Components = new List<InternalComponent>();
        public bool Enabled { get; set; }

        public InternalComponent(bool enabled, string name, string val) : base(name, val)
        {
            Enabled = enabled;
            Components.Add(this);
            ValueLabel.IsMonospaced = true;
        }

        public new float MinimumWidth => Enabled ? base.MinimumWidth : 0f;
        public new float MinimumHeight => Enabled ? base.MinimumHeight : 0f;
        public new float PaddingTop => Enabled ? base.PaddingTop : 0f;
        public new float PaddingBottom => Enabled ? base.PaddingBottom : 0f;
        public new float PaddingLeft => Enabled ? base.PaddingLeft : 0f;
        public new float PaddingRight => Enabled ? base.PaddingRight : 0f;
        public new float VerticalHeight => Enabled ? base.VerticalHeight : 0f;
        public new float HorizontalWidth => Enabled ? base.HorizontalWidth : 0f;

        public new void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            if (Enabled)
                base.DrawHorizontal(g, state, height, clipRegion);
        }

        public new void DrawVertical(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            if (Enabled)
                base.DrawVertical(g, state, height, clipRegion);
        }

        public override void PrepareDraw(LiveSplitState state, LayoutMode mode)
        {
            base.PrepareDraw(state, mode);
            ValueLabel.IsMonospaced = true;
        }

        public void SetText(string text)
        {
            InformationValue = text;
        }

        public void SetName(string name)
        {
            InformationName = name;
        }
    }

    public class InternalTextComponent : InternalComponent
    {
        public InternalTextComponent(bool enabled, string text, string val) : base(enabled, text, val)
        {
            ValueLabel.IsMonospaced = true;
        }
    }

    public class InternalTimeComponent : InternalComponent
    {
        public InternalTimeComponent(bool enabled, string name, string val) : base(enabled, name, val)
        {
            ValueLabel.IsMonospaced = true;
        }

        public void SetTime(TimeSpan time, int precision = 6)
        {
            /*
            InformationValue =
                (time < TimeSpan.Zero ? "-" : "") +
                time.ToString($@"h\:mm\:ss\.{new string('f', precision)}");
            */

            InformationValue = time.ToStringCustom(precision);
        }

        public override void PrepareDraw(LiveSplitState state, LayoutMode mode)
        {
            ValueLabel.Font = state.LayoutSettings.TimesFont;
            NameMeasureLabel.Font = state.LayoutSettings.TextFont;
            NameLabel.Font = state.LayoutSettings.TextFont;
            if (mode == LayoutMode.Vertical)
            {
                NameLabel.VerticalAlignment = StringAlignment.Center;
                ValueLabel.VerticalAlignment = StringAlignment.Center;
            }
            else
            {
                NameLabel.VerticalAlignment = StringAlignment.Near;
                ValueLabel.VerticalAlignment = StringAlignment.Far;
            }
        }
    }

    // robbed from livesplit
    public class InternalComponentRenderer
    {
        public List<InternalComponent> EnabledComponents => InternalComponent.Components.Where(x => x.Enabled).ToList();
        public List<InternalComponent> VisibleComponents => EnabledComponents;

        public float OverallSize = 10f;

        public float MinimumWidth => !EnabledComponents.Any() ? 0 : EnabledComponents.Sum(x => x.MinimumWidth);
        public float MinimumHeight => !EnabledComponents.Any() ? 0 : EnabledComponents.Sum(x => x.MinimumHeight);
        public float VerticalHeight => EnabledComponents.Sum(x => x.VerticalHeight);
        public float HorizontalWidth => EnabledComponents.Sum(x => x.HorizontalWidth);
        public float PaddingLeft => EnabledComponents.Sum(x => x.PaddingLeft);
        public float PaddingRight => EnabledComponents.Sum(x => x.PaddingRight);
        public float PaddingTop => EnabledComponents.Sum(x => x.PaddingTop);
        public float PaddingBottom => EnabledComponents.Sum(x => x.PaddingBottom);

        private void DrawVerticalComponent(int index, Graphics g, LiveSplitState state, float width, float height, Region clipRegion)
        {
            var component = VisibleComponents.ElementAt(index);
            var topPadding = Math.Min(GetPaddingAbove(index), component.PaddingTop) / 2f;
            var bottomPadding = Math.Min(GetPaddingBelow(index), component.PaddingBottom) / 2f;
            g.IntersectClip(new RectangleF(0, topPadding, width, component.VerticalHeight - topPadding - bottomPadding));

            var scale = g.Transform.Elements.First();
            var separatorOffset = component.VerticalHeight * scale < 3 ? 1 : 0;

            if (clipRegion.IsVisible(new RectangleF(
                g.Transform.OffsetX,
                -separatorOffset + g.Transform.OffsetY - topPadding * scale,
                width,
                separatorOffset * 2f + scale * (component.VerticalHeight + bottomPadding))))
                component.DrawVertical(g, state, width, clipRegion);
            g.TranslateTransform(0.0f, component.VerticalHeight - bottomPadding * 2f);
        }

        private void DrawHorizontalComponent(int index, Graphics g, LiveSplitState state, float width, float height, Region clipRegion)
        {
            var component = VisibleComponents.ElementAt(index);
            var leftPadding = Math.Min(GetPaddingToLeft(index), component.PaddingLeft) / 2f;
            var rightPadding = Math.Min(GetPaddingToRight(index), component.PaddingRight) / 2f;
            g.IntersectClip(new RectangleF(leftPadding, 0, component.HorizontalWidth - leftPadding - rightPadding, height));

            var scale = g.Transform.Elements.First();
            var separatorOffset = component.VerticalHeight * scale < 3 ? 1 : 0;

            if (clipRegion.IsVisible(new RectangleF(
                -separatorOffset + g.Transform.OffsetX - leftPadding * scale,
                g.Transform.OffsetY,
                separatorOffset * 2f + scale * (component.HorizontalWidth + rightPadding),
                height)))
                component.DrawHorizontal(g, state, height, clipRegion);
            g.TranslateTransform(component.HorizontalWidth - rightPadding * 2f, 0.0f);
        }

        private float GetPaddingAbove(int index)
        {
            while (index > 0)
            {
                index--;
                var component = VisibleComponents.ElementAt(index);
                if (component.VerticalHeight != 0)
                    return component.PaddingBottom;
            }
            return 0f;
        }

        private float GetPaddingBelow(int index)
        {
            while (index < VisibleComponents.Count() - 1)
            {
                index++;
                var component = VisibleComponents.ElementAt(index);
                if (component.VerticalHeight != 0)
                    return component.PaddingTop;
            }
            return 0f;
        }

        private float GetPaddingToLeft(int index)
        {
            while (index > 0)
            {
                index--;
                var component = VisibleComponents.ElementAt(index);
                if (component.HorizontalWidth != 0)
                    return component.PaddingLeft;
            }
            return 0f;
        }

        private float GetPaddingToRight(int index)
        {
            while (index < VisibleComponents.Count() - 1)
            {
                index++;
                var component = VisibleComponents.ElementAt(index);
                if (component.HorizontalWidth != 0)
                    return component.PaddingRight;
            }
            return 0f;
        }

        protected float GetHeightVertical(int index)
        {
            var component = VisibleComponents.ElementAt(index);
            var bottomPadding = Math.Min(GetPaddingBelow(index), component.PaddingBottom) / 2f;
            return component.VerticalHeight - bottomPadding * 2f;
        }

        protected float GetWidthHorizontal(int index)
        {
            var component = VisibleComponents.ElementAt(index);
            var rightPadding = Math.Min(GetPaddingToRight(index), component.PaddingRight) / 2f;
            return component.HorizontalWidth - rightPadding * 2f;
        }

        public void CalculateOverallSize(LayoutMode mode)
        {
            var totalSize = 0f;
            var index = 0;
            foreach (var component in VisibleComponents)
            {
                if (!component.Enabled)
                    continue;

                if (mode == LayoutMode.Vertical)
                    totalSize += GetHeightVertical(index);
                else
                    totalSize += GetWidthHorizontal(index);
                index++;
            }

            OverallSize = Math.Max(totalSize, 1f);
        }

        public void Render(Graphics g, LiveSplitState state, float width, float height, LayoutMode mode, Region clipRegion)
        {
            var clip = g.Clip;
            var transform = g.Transform;
            var index = 0;
            foreach (var component in VisibleComponents)
            {
                g.Clip = clip;
                if (mode == LayoutMode.Vertical)
                    DrawVerticalComponent(index, g, state, width, height, clipRegion);
                else
                    DrawHorizontalComponent(index, g, state, width, height, clipRegion);
                index++;
            }
            g.Transform = transform;
            g.Clip = clip;
        }

        protected void InvalidateVerticalComponent(int index, LiveSplitState state, IInvalidator invalidator, float width, float height, float scaleFactor)
        {
            var component = VisibleComponents.ElementAt(index);
            var topPadding = Math.Min(GetPaddingAbove(index), component.PaddingTop) / 2f;
            var bottomPadding = Math.Min(GetPaddingBelow(index), component.PaddingBottom) / 2f;
            var totalHeight = scaleFactor * (component.VerticalHeight - topPadding - bottomPadding);
            component.Update(invalidator, state, width, totalHeight, LayoutMode.Vertical);
            invalidator.Transform.Translate(0.0f, totalHeight);
        }

        protected void InvalidateHorizontalComponent(int index, LiveSplitState state, IInvalidator invalidator, float width, float height, float scaleFactor)
        {
            var component = VisibleComponents.ElementAt(index);
            var leftPadding = Math.Min(GetPaddingToLeft(index), component.PaddingLeft) / 2f;
            var rightPadding = Math.Min(GetPaddingToRight(index), component.PaddingRight) / 2f;
            var totalWidth = scaleFactor * (component.HorizontalWidth - leftPadding - rightPadding);
            component.Update(invalidator, state, totalWidth, height, LayoutMode.Horizontal);
            invalidator.Transform.Translate(totalWidth, 0.0f);
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (invalidator == null)
                return;

            var oldTransform = invalidator.Transform.Clone();
            var scaleFactor = mode == LayoutMode.Vertical
                    ? height / OverallSize
                    : width / OverallSize;

            for (var ind = 0; ind < VisibleComponents.Count(); ind++)
            {
                if (mode == LayoutMode.Vertical)
                    InvalidateVerticalComponent(ind, state, invalidator, width, height, scaleFactor);
                else
                    InvalidateHorizontalComponent(ind, state, invalidator, width, height, scaleFactor);
            }
            invalidator.Transform = oldTransform;
        }
    }
}

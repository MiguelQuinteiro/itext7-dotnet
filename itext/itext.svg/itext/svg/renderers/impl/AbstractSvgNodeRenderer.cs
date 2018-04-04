using System;
using System.Collections.Generic;
using iText.IO.Util;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.StyledXmlParser.Css.Util;
using iText.Svg;
using iText.Svg.Renderers;
using iText.Svg.Utils;

namespace iText.Svg.Renderers.Impl {
    /// <summary><inheritDoc/></summary>
    public abstract class AbstractSvgNodeRenderer : ISvgNodeRenderer {
        private ISvgNodeRenderer parent;

        private readonly IList<ISvgNodeRenderer> children = new List<ISvgNodeRenderer>();

        protected internal IDictionary<String, String> attributesAndStyles;

        private bool doFill = false;

        public virtual void SetParent(ISvgNodeRenderer parent) {
            this.parent = parent;
        }

        public virtual ISvgNodeRenderer GetParent() {
            return parent;
        }

        public void AddChild(ISvgNodeRenderer child) {
            // final method, in order to disallow adding null
            if (child != null) {
                children.Add(child);
            }
        }

        public IList<ISvgNodeRenderer> GetChildren() {
            // final method, in order to disallow modifying the List
            return JavaCollectionsUtil.UnmodifiableList(children);
        }

        public virtual void SetAttributesAndStyles(IDictionary<String, String> attributesAndStyles) {
            this.attributesAndStyles = attributesAndStyles;
        }

        /// <summary>
        /// Applies transformations set to this object, if any, and delegates the drawing of this element and its children
        /// to the
        /// <see cref="DoDraw(iText.Svg.Renderers.SvgDrawContext)">doDraw</see>
        /// method.
        /// </summary>
        /// <param name="context">the object that knows the place to draw this element and maintains its state</param>
        public void Draw(SvgDrawContext context) {
            PdfCanvas currentCanvas = context.GetCurrentCanvas();
            if (this.attributesAndStyles != null) {
                String transformString = this.attributesAndStyles.Get(SvgTagConstants.TRANSFORM);
                if (transformString != null && !String.IsNullOrEmpty(transformString)) {
                    AffineTransform transformation = TransformUtils.ParseTransform(transformString);
                    currentCanvas.ConcatMatrix(transformation);
                }
            }
            PreDraw(context);
            DoDraw(context);
            PostDraw(context);
            if (attributesAndStyles != null && attributesAndStyles.ContainsKey(SvgTagConstants.ID)) {
                context.AddNamedObject(attributesAndStyles.Get(SvgTagConstants.ID), this);
            }
        }

        /// <summary>Operations to perform before drawing an element.</summary>
        /// <remarks>
        /// Operations to perform before drawing an element.
        /// This includes setting stroke color and width, fill color.
        /// </remarks>
        /// <param name="context">the svg draw context</param>
        internal virtual void PreDraw(SvgDrawContext context) {
            if (this.attributesAndStyles != null) {
                PdfCanvas currentCanvas = context.GetCurrentCanvas();
 {
                    // fill
                    String fillRawValue = GetAttribute(SvgTagConstants.FILL);
                    this.doFill = !SvgTagConstants.NONE.EqualsIgnoreCase(fillRawValue);
                    if (doFill && CanElementFill()) {
                        // todo RND-865 default style sheets
                        Color color = ColorConstants.BLACK;
                        if (fillRawValue != null) {
                            color = WebColors.GetRGBColor(fillRawValue);
                        }
                        currentCanvas.SetFillColor(color);
                    }
                }
 {
                    // stroke
                    String strokeRawValue = GetAttribute(SvgTagConstants.STROKE);
                    DeviceRgb rgbColor = WebColors.GetRGBColor(strokeRawValue);
                    if (strokeRawValue != null && rgbColor != null) {
                        currentCanvas.SetStrokeColor(rgbColor);
                        String strokeWidthRawValue = GetAttribute(SvgTagConstants.STROKE_WIDTH);
                        float strokeWidth = 1f;
                        if (strokeWidthRawValue != null) {
                            strokeWidth = CssUtils.ParseAbsoluteLength(strokeWidthRawValue);
                        }
                        currentCanvas.SetLineWidth(strokeWidth);
                    }
                }
            }
        }

        protected internal virtual bool CanElementFill() {
            return true;
        }

        /// <summary>Operations to be performed after drawing the element.</summary>
        /// <remarks>
        /// Operations to be performed after drawing the element.
        /// This includes filling, stroking.
        /// </remarks>
        /// <param name="context">the svg draw context</param>
        internal virtual void PostDraw(SvgDrawContext context) {
            if (this.attributesAndStyles != null) {
                PdfCanvas currentCanvas = context.GetCurrentCanvas();
                // fill-rule
                if (doFill && CanElementFill()) {
                    String fillRuleRawValue = GetAttribute(SvgTagConstants.FILL_RULE);
                    if (SvgTagConstants.FILL_RULE_EVEN_ODD.EqualsIgnoreCase(fillRuleRawValue)) {
                        // TODO RND-878
                        currentCanvas.EoFill();
                    }
                    else {
                        currentCanvas.Fill();
                    }
                }
                if (GetAttribute(SvgTagConstants.STROKE) != null) {
                    currentCanvas.Stroke();
                }
                currentCanvas.ClosePath();
            }
        }

        /// <summary>Draws this element to a canvas-like object maintained in the context.</summary>
        /// <param name="context">the object that knows the place to draw this element and maintains its state</param>
        protected internal abstract void DoDraw(SvgDrawContext context);

        public virtual String GetAttribute(String key) {
            return attributesAndStyles.Get(key);
        }

        public virtual void SetAttribute(String key, String value) {
            this.attributesAndStyles.Put(key, value);
        }
    }
}

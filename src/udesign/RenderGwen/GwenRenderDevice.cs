﻿using Gwen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using udesign.Render;
using ulib;
using ulib.Controls;
using ulib.Elements;

namespace udesign
{
    public class GwenRenderDevice : RenderDevice
    {
        public void RenderNode(Node node, RenderContext rc)
        {
            GwenRenderContext grc = rc as GwenRenderContext;
            if (grc == null)
                return;

            if (node is ImageNode)
            {
                RenderImageNode(node as ImageNode, grc);
            }
            else if (node is TextNode)
            {
                RenderTextNode(node as TextNode, grc);
            }
            else if (node is Button)
            {
                RenderButton(node as Button, grc);
            }
            else if (node is CheckBox)
            {
                RenderCheckBox(node as CheckBox, grc);
            }
            else
            {
                Rectangle rect = new Rectangle(grc.GetAccumulatedDockedTranslate(), node.Size);
                grc.m_renderer.DrawLinedRect(rect);
            }
        }

        private void RenderImageNode(ImageNode imageNode, GwenRenderContext grc)
        {
            DrawImage(grc, new Rectangle(grc.GetAccumulatedDockedTranslate(), imageNode.Size), imageNode.Res);
        }

        private void RenderTextNode(TextNode textNode, GwenRenderContext grc)
        {
            // 当需要的时候，先更新 TextNode 的尺寸
            Point textSize = grc.m_renderer.MeasureText(grc.m_font, textNode.Text);
            if (textNode.RequestedSizeRefreshing)
            {
                textNode.Size = new Size(
                    Math.Max(textNode.Size.Width, textSize.X),
                    Math.Max(textNode.Size.Height, textSize.Y));
                NodeSGUtil.ClampBounds(textNode);
                textNode.RequestedSizeRefreshing = false;
            }

            Point loc = grc.GetAccumulatedDockedTranslate();

            // 理论上这里我们不应当每帧对每段 Text 都调用 MeasureText()
            // 不过考察 Gwen.Renderer.Tao.MeasureText() 后我们发现 
            // 其内部已经 Cache 了一份，把尺寸再缓存一份意义不大
            Point internalOffset = TextNodeUtil.CalculateInternalTextOffset(textNode, textSize);
            loc.Offset(internalOffset);

            Color c = grc.m_renderer.DrawColor;
            grc.m_renderer.DrawColor = textNode.TextColor;
            grc.m_renderer.RenderText(grc.m_font, loc, textNode.Text);
            grc.m_renderer.DrawColor = c;
        }

        private void RenderButton(Button bt, GwenRenderContext grc)
        {
            DrawImage(grc, new Rectangle(grc.GetAccumulatedDockedTranslate(), bt.Size), bt.Res_Background);
        }

        private void RenderCheckBox(CheckBox cb, GwenRenderContext grc)
        {
            Rectangle rect = new Rectangle(grc.GetAccumulatedDockedTranslate(), cb.MarkSize);
            DrawImage(grc, rect, cb.Res_Background);
            DrawImage(grc, rect, cb.Res_Mark);
        }

        private void DrawImage(GwenRenderContext grc, Rectangle rect, string url)
        {
            TextureRenderInfo tri = GwenTextureProvider.Instance.GetTextureRenderInfo(grc.m_renderer, url);
            if (tri != null) // 找不到贴图的话，正常的处理应该用一个显眼的错误图案，这里暂时先忽略，待补充
            {
                grc.m_renderer.DrawTexturedRect(tri.texture, rect,
                    tri.u1,
                    tri.v1,
                    tri.u2,
                    tri.v2);
            }
        }
    }
}
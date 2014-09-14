﻿using Gwen.ControlInternal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using udesign;
using ulib;
using ulib.Elements;

namespace udesign
{
    public partial class SceneEd
    {
        public static SceneEd Instance = new SceneEd();

        public bool IsHoldingCtrl { get; set; }

        public SelectionList Selection { get { return m_selectionList; } }
        public bool HasSelection { get { return Selection.Selection.Count != 0; } }

        public OperationHistory OperHistory { get { return m_operHistory; } }
        public DragAndDropReceiver DragAndDrop { get { return m_dragAndDropReceiver; } }
        public SceneClipboard Clipboard { get { return m_clipboard; } }

        public void Render(Gwen.Renderer.Tao renderer, GwenRenderContext ctx)
        {
            m_dragAndDropReceiver.Render(renderer, ctx);
            m_selectionList.Render(renderer, ctx);
        }

        public void MouseDown(MouseEventArgs e)
        {
            Node pickedNode = Scene.Instance.Pick(e.Location);
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (m_selectionList.Selection.Contains(pickedNode) && m_selectionList.IsSelectionDraggable())
                    {
                        DragLeft_BeginMoving(e.Location);
                    }
                    break;
                case MouseButtons.Right:
                    if (m_selectionList.Selection.Contains(pickedNode) && m_selectionList.Selection.Count == 1 && pickedNode.IsScrollable())
                    {
                        DragRight_BeginScrolling(e.Location);
                    }
                    break;
                default:
                    break;
            }
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (IsDraggingLeft())
            {
                DragLeft_UpdateMoving(e.Location);
            }
            else if (IsDragRightScrolling())
            {
                DragRight_UpdateScrolling(e.Location);
            }
        }

        public void MouseUp(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (IsDraggingLeft())
                    {
                        DragLeft_EndMoving(e.Location);
                    }
                    else
                    {
                        Node n = Scene.Instance.Pick(e.Location);
                        Select(n);
                    }
                    break;
                case MouseButtons.Right:
                    if (IsDragRightScrolling())
                    {
                        DragRight_EndScrolling(e.Location);
                    }
                    break;
                default:
                    break;
            }
            SceneEdEventNotifier.Instance.Emit_RefreshScene(RefreshSceneOpt.Refresh_All);
        }

        public void Undo()
        {
            m_operHistory.Undo();
            SceneEdEventNotifier.Instance.Emit_RefreshScene(RefreshSceneOpt.Refresh_All);
        }

        public void Redo()
        {
            m_operHistory.Redo();
            SceneEdEventNotifier.Instance.Emit_RefreshScene(RefreshSceneOpt.Refresh_All);
        }

        public void Select(Node n)
        {
            if (!IsHoldingCtrl)
            {
                m_selectionList.Selection.Clear();
            }
            if (n != null)
            {
                m_selectionList.Selection.Add(n);
                SceneEdEventNotifier.Instance.Emit_SelectNode(n, this);
            }
            else
            {
                SceneEdEventNotifier.Instance.Emit_SelectNode(null, this);
            }

            m_selectionList.OnSelectionChanged();
            SceneEdEventNotifier.Instance.Emit_RefreshScene(RefreshSceneOpt.Refresh_Rendering);
        }

        public void Select(List<Node> nodes)
        {
            if (nodes.Count == 0)
                return;

            m_selectionList.Selection.Clear();
            foreach (var item in nodes)
            {
                m_selectionList.Selection.Add(item);
                SceneEdEventNotifier.Instance.Emit_SelectNode(item, this);
            }

            m_selectionList.OnSelectionChanged();
            SceneEdEventNotifier.Instance.Emit_RefreshScene(RefreshSceneOpt.Refresh_Rendering);
        }

        public void DeleteSelected()
        {
            if (m_selectionList.HasSelectedRoot())
            {
                Session.Message("无法删除根节点。");                
            }
            else
            {
                m_operHistory.PushAction(new Action_Delete(m_selectionList.Selection));
                m_selectionList.ClearSelection();
            }
        }

        private OperationHistory m_operHistory = new OperationHistory();
        private SelectionList m_selectionList = new SelectionList();
        private DragAndDropReceiver m_dragAndDropReceiver = new DragAndDropReceiver();
        private SceneClipboard m_clipboard = new SceneClipboard();

        internal void InitSelectionContainer(Gwen.Control.Canvas canvas)
        {
            m_selectionList.Init(canvas);
            m_selectionList.Resizer.Resized += Resizer_Resized;
            for (int i = 0; i < 10; i++)
            {
                Resizer r = m_selectionList.Resizer.GetInternalResizer(i);
                if (r == null)
                    continue;

                r.BeginDrag += Resizer_Begin;
                r.EndDrag += Resizer_End;
            }
        }

        public void Cut()
        {
            if (HasSelection)
            {
                Clipboard.SetClippedContent(m_selectionList.Selection, true);
            }
        }

        public void Copy()
        {
            if (HasSelection)
            {
                Clipboard.SetClippedContent(m_selectionList.Selection, false);
            }
        }

        public void Paste()
        {
            if (HasSelection && Clipboard.IsInUse)
            {
                List<Node> newlyCreated = SceneEd.Instance.Clipboard.AttachTo(SceneEd.Instance.Selection.Selection[0]);
                SceneEd.Instance.Select(newlyCreated);
                SceneEdEventNotifier.Instance.Emit_RefreshScene(RefreshSceneOpt.Refresh_All);
            }
        }
    }
}

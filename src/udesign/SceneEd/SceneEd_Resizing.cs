﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace udesign
{
    public partial class SceneEd
    {
        Action_Resize m_resizeAction;

        void Resizer_Begin(Gwen.Control.Base sender, EventArgs arguments)
        {
            if (m_selectionList.Resizer.IsHoveringResizers() && m_selectionList.Selection.Count == 1)
            {
                m_resizeAction = new Action_Resize(m_selectionList.Selection[0]);
            }
        }

        void Resizer_End(Gwen.Control.Base sender, EventArgs arguments)
        {
            if (m_resizeAction != null)
            {
                m_resizeAction.EndResizing(m_selectionList.Resizer.Bounds);
                m_operHistory.PushAction(m_resizeAction);
                m_resizeAction = null;
            }
        }

        void Resizer_Resized(Gwen.Control.Base sender, EventArgs arguments)
        {
            if (m_resizeAction != null)
            {
                m_resizeAction.UpdateResizing(m_selectionList.Resizer.Bounds);
            }
        }
    }
}

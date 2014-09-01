﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using ulib.Base;
using ulib.Elements;

namespace ulib.Elements
{
    public class RootNode : Node
    {
        [Category("Root")]
        public bool IsFullscren 
        { 
            get 
            { 
                return m_isFullscreen; 
            } 
            set 
            { 
                m_isFullscreen = value;
                OnFullscreenChanged(); 
            }
        }

        [Category("Root")]
        [JsonIgnore]
        public Resolution.Slot CurrentResolution
        {
            get
            {
                return m_settings.EditTimeResSlot;
            }
            set
            {
                m_settings.SetSlot(value);
                OnFullscreenChanged();
            }
        }

        public RootNode()
        {
            base.m_parent = null;
            base.Name = Default_Name;

            UserData = m_settings;

            OnFullscreenChanged();
        }

        protected void OnFullscreenChanged()
        {
            if (IsFullscren)
            {
                Position = Constants.ZeroPoint;
                Size = m_settings.EditTimeResolution;
            }
            else
            {
                Rectangle childrenWorldBounds = GetChildrenWorldBounds();
                if (Base.Math.IsInvalid(childrenWorldBounds))
                {
                    Position = Default_Position;
                    Size = Default_Size;
                }
                else
                {
                    Position = Constants.ZeroPoint;
                    Size = new Size(childrenWorldBounds.Right, childrenWorldBounds.Bottom);
                }
            }
        }

        protected bool m_isFullscreen = false;
        protected RootNodeSettings m_settings = new RootNodeSettings();

        public static readonly Point Default_Position = new Point { X = 100, Y = 100 };
        public static readonly Size Default_Size = new Size { Width = 600, Height = 500 };
        public static readonly string Default_Name = "Root";
    }
}
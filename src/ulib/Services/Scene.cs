﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ulib;
using ulib.Base;
using ulib.Elements;

namespace ulib
{
    public class Scene : IDisposable
    {
        public static Scene Instance;

        /// <summary>
        /// 只读属性
        /// </summary>
        public RootNode Root { get { return m_root; } }

        public ResolutionV2 DesignTimeResolution;

        public Scene()
        {
        }

        public bool Init(ResolutionV2 designTimeResolution)
        {
            DesignTimeResolution = designTimeResolution;
            m_root = new RootNode();
            return true;
        }

        public void Dispose()
        {

        }

        public bool Load(string targetLocation)
        {
            Node loaded = m_archiveSys.Load(targetLocation);
            if (loaded == null || !(loaded is RootNode))
            {
                Session.Log("Scene.Load 加载失败. '{0}'", targetLocation);
                return false;
            }

            m_currentFilePath = targetLocation;
            m_root = loaded as RootNode;
            return true;
        }

        public bool Save()
        {
            if (string.IsNullOrEmpty(m_currentFilePath))
            {
                Session.Log("保存文件时 m_currentFilePath 无效，且未传入有效的路径。");
                return false;
            }

            return m_archiveSys.Save(m_root, m_currentFilePath);
        }

        public bool Save(string targetLocation)
        {
            if (!m_archiveSys.Save(m_root, targetLocation))
                return false;

            m_currentFilePath = targetLocation;
            return true;
        }

        public void Render(RenderContext rc, RenderDevice rs)
        {
            m_renderSys.Render(m_root, rc, rs);
        }

        public Node Pick(Point location)
        {
            return SceneGraphUtil.Pick(m_root, location);
        }

        public string CurrentFilePath { get { return m_currentFilePath; } }

        private string m_currentFilePath;
        private RootNode m_root;
        private RenderSystem m_renderSys = new RenderSystem();
        private ArchiveSystem m_archiveSys = new ArchiveSystem();
    }
}

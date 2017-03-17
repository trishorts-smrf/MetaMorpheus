﻿using TaskLayer;

namespace MetaMorpheusGUI
{
    internal class ProteinDbForDataGrid
    {

        #region Public Constructors

        public ProteinDbForDataGrid(string fileName)
        {
            Use = true;
            FileName = fileName;
            if (fileName.ToUpper().Contains("contaminant".ToUpper())
                || fileName.ToUpper().Contains("crap".ToUpper()))
                Contaminant = true;
        }

        public ProteinDbForDataGrid(DbForTask uu)
        {
            Use = true;
            Contaminant = uu.IsContaminant;
            FileName = uu.FileName;
        }

        #endregion Public Constructors

        #region Public Properties

        public bool Use { get; set; }
        public bool Contaminant { get; set; }
        public string FileName { get; }
        public bool InProgress { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Method to mark as in progress. Need the property setter to be private so user could not check off in GUI
        /// </summary>
        /// <param name="inProgress"></param>
        public void SetInProgress(bool inProgress)
        {
            InProgress = inProgress;
        }

        #endregion Public Methods

    }
}
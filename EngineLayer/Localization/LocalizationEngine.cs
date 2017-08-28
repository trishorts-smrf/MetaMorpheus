﻿using MassSpectrometry;
using MzLibUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EngineLayer
{
    public class LocalizationEngine : MetaMorpheusEngine
    {
        #region Private Fields

        private readonly IEnumerable<Psm> allResultingIdentifications;
        private readonly List<ProductType> lp;
        private readonly IMsDataFile<IMsDataScan<IMzSpectrum<IMzPeak>>> myMsDataFile;
        private readonly Tolerance fragmentTolerance;
        private readonly bool addCompIons;

        #endregion Private Fields

        #region Public Constructors

        public LocalizationEngine(IEnumerable<Psm> allResultingIdentifications, List<ProductType> lp, IMsDataFile<IMsDataScan<IMzSpectrum<IMzPeak>>> myMsDataFile, Tolerance fragmentTolerance, List<string> nestedIds, bool addCompIons) : base(nestedIds)
        {
            this.allResultingIdentifications = allResultingIdentifications;
            this.lp = lp;
            this.myMsDataFile = myMsDataFile;
            this.fragmentTolerance = fragmentTolerance;
            this.addCompIons = addCompIons;
        }

        #endregion Public Constructors

        #region Protected Methods

        protected override MetaMorpheusEngineResults RunSpecific()
        {
            TerminusType terminusType = ProductTypeToTerminusType.IdentifyTerminusType(lp);

            foreach (var ok in allResultingIdentifications)
            {
                var matchedIonDictOnlyMatches = new Dictionary<ProductType, double[]>();
                var theScan = myMsDataFile.GetOneBasedScan(ok.ScanNumber);
                double thePrecursorMass = ok.ScanPrecursorMass;
                foreach (var huh in lp)
                {
                    var ionMasses = ok.CompactPeptides.First().Key.ProductMassesMightHaveDuplicatesAndNaNs(new List<ProductType> { huh });
                    Array.Sort(ionMasses);
                    double[] matchedIonMassesListPositiveIsMatch = new double[ionMasses.Length];
                    MatchIons(theScan, fragmentTolerance, ionMasses, matchedIonMassesListPositiveIsMatch, this.addCompIons, thePrecursorMass, this.lp);
                    double[] matchedIonMassesOnlyMatches = matchedIonMassesListPositiveIsMatch.Where(m => m > 0).ToArray();
                    matchedIonDictOnlyMatches.Add(huh, matchedIonMassesOnlyMatches);
                }

                ok.MatchedIonDictPositiveIsMatch = new MatchedIonMassesListOnlyMasses(matchedIonDictOnlyMatches);
            }

            foreach (var ok in allResultingIdentifications.Where(b => b.NumDifferentCompactPeptides == 1))
            {
                var theScan = myMsDataFile.GetOneBasedScan(ok.ScanNumber);
                double thePrecursorMass = ok.ScanPrecursorMass;

                if (ok.FullSequence == null)
                    continue;

                var representative = ok.CompactPeptides.First().Value.Item2.First();

                var localizedScores = new List<double>();
                for (int indexToLocalize = 0; indexToLocalize < representative.Length; indexToLocalize++)
                {
                    PeptideWithSetModifications localizedPeptide = representative.Localize(indexToLocalize, ok.ScanPrecursorMass - representative.MonoisotopicMass);

                    var gg = localizedPeptide.CompactPeptide(terminusType).ProductMassesMightHaveDuplicatesAndNaNs(lp);
                    Array.Sort(gg);
                    double[] matchedIonMassesListPositiveIsMatch = new double[gg.Length];
                    var score = MatchIons(theScan, fragmentTolerance, gg, matchedIonMassesListPositiveIsMatch, this.addCompIons, thePrecursorMass, this.lp);
                    localizedScores.Add(score);
                }

                ok.LocalizedScores = localizedScores;
            }
            return new LocalizationEngineResults(this);
        }

        #endregion Protected Methods
    }
}
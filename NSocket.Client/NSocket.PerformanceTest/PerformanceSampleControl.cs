// -----------------------------------------------------------------------
// <copyright file="PerformanceSampleControl.cs" company="ComponentOwl.com">
//     Copyright © 2010-2013 ComponentOwl.com. All rights reserved.
// </copyright>
// <author>Libor Tinka</author>
// -----------------------------------------------------------------------

namespace ComponentOwl.BetterListView.Samples.CSharp
{
    #region Usings

    using System.ComponentModel;

    using ComponentOwl.Samples;

    #endregion

    [SampleControlAttribute(
        "Performance",
        Program.GroupMainFeatures,
        "Improving performance of the control.")]

    [ToolboxItem(false)]
    internal sealed class PerformanceSampleControl : SampleControl
    {
        public PerformanceSampleControl()
        {
            PerformanceSampleContentControl contentControl = new PerformanceSampleContentControl();

            SetControls(
                contentControl.ListView,
                contentControl,
                null);
        }
    }
}
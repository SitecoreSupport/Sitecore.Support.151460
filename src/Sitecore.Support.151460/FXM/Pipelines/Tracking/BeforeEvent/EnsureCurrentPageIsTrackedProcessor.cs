using Sitecore.Analytics.Tracking;
using Sitecore.Diagnostics;
using Sitecore.FXM.Pipelines.Tracking;
using Sitecore.FXM.Tracking;

namespace Sitecore.Support.FXM.Pipelines.Tracking.BeforeEvent
{
    public class EnsureCurrentPageIsTrackedProcessor : Sitecore.FXM.Pipelines.Tracking.BeforeEvent.EnsureCurrentPageIsTrackedProcessor
    {

        public new void Process(ITrackingArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.TrackerProvider, "TrackerProvider");
            Assert.ArgumentNotNull(args.TrackerProvider.Current, "Current tracker provider");
            Assert.ArgumentNotNull(args.TrackingRequest, "TrackingRequest");
            if (args.CurrentPageVisit == null)
            {
                if (!args.TrackerProvider.Current.IsActive && (args.TrackerProvider.Current.Interaction == null))
                {
                    args.TrackerProvider.Current.StartTracking();
                }

                //patch start
                if (!args.TrackerProvider.Current.IsActive)
                {
                    args.TrackerProvider.Current.Interaction.ValueAggregatedEvent+=
                         delegate (int value) {
                             if (args.TrackerProvider.Current.Session.Contact != null)
                             {
                                 IContactSystemInfoContext system = args.TrackerProvider.Current.Contact.System;
                                 system.Value += value;
                             }
                         };
                }
                //patch end

                if (args.TrackerProvider.Current.Interaction != null)
                {
                    IPageContext currentPageInInteraction = this.GetCurrentPageInInteraction(args.TrackerProvider.Current.Interaction, args.TrackingRequest.Url);
                    if (currentPageInInteraction != null)
                    {
                        args.CurrentPageVisit = currentPageInInteraction;
                    }
                }
                if (args.CurrentPageVisit == null)
                {
                    args.AbortAndFailPipeline("the current page has not been tracked in the current session.", TrackingResultCode.CurrentPageMustBeTracked);
                }
            }
        }
    }
}

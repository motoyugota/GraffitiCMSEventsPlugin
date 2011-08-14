using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graffiti.Core;

namespace Graffiti.Plugins.Events
{
	public static class EventExtensions
	{
		public static DateTime GetEventDate(this Post post)
		{
			return CalendarFunctions.TryDateTimeParse(post.Custom("Event Date")).Date; // For backwards compatibility - the EventDate used to be saved with a time
		}
		public static DateTime GetStartDate(this Post post)
		{
			return CalendarFunctions.TryDateTimeParse(post.Custom("Start Date"));
		}
		public static DateTime GetEndDate(this Post post)
		{
			return CalendarFunctions.TryDateTimeParse(post.Custom("End Date"));
		}

		public static DateTime GetEffectiveDate(this Post post)
		{
			DateTime eventDate = GetEventDate(post);
			if (eventDate == DateTime.MinValue)
			{
				return GetStartDate(post);
			}

			return eventDate;
		}

		public static bool IsOnDate(this Post post, DateTime date)
		{
			return (post.GetEventDate() == date.Date || (date.Date >= post.GetStartDate() && date.Date <= post.GetEndDate()));
		}

		public static bool IsInFuture(this Post post)
		{
			return post.GetEventDate() >= DateTime.Today || post.GetStartDate() >= DateTime.Today;
		}

		public static bool IsInPast(this Post post)
		{
			return (post.GetEventDate() != DateTime.MinValue && post.GetEventDate() < DateTime.Today) || (post.GetEndDate() != DateTime.MinValue && post.GetEndDate() < DateTime.Today);
		}

		public static bool IsInRange(this Post post, DateTime startDate, DateTime endDate)
		{
			DateTime eventDate = post.GetEventDate();
			if (eventDate != DateTime.MinValue)
			{
				return (eventDate >= startDate && eventDate <= endDate);
			}
			else
			{
				DateTime eventStartDate = post.GetStartDate();
				DateTime eventEndDate = post.GetEndDate();
				return ((eventStartDate >= startDate && eventStartDate <= endDate)
					|| (eventEndDate >= startDate && eventEndDate <= endDate));
			}
		}
	}
}

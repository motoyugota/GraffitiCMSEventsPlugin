/* ****************************************************************************
 *
 * Copyright (c) Nexus Technologies, LLC. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found at:
 * 
 * http://graffitirssextension.codeplex.com/license
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataBuddy;
using Graffiti.Core;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.IO;

namespace Graffiti.Plugins.Events
{
	[Chalk("events")]
	[Serializable()]
	public class Events
	{
		public DateTime CalendarDate
		{
			get
			{
				int year = 0;
				int month = 0;

				CalendarFunctions.GetMonth(ref year, ref month);

				return new DateTime(year, month, 1);
			}
		}

		public DateTime EventDate(Post post)
		{
			return post.GetEventDate();
		}

		public DateTime StartDate(Post post)
		{
			return post.GetStartDate();
		}

		public DateTime EndDate(Post post)
		{
			return post.GetEndDate();
		}

		public string PostMonthUrl(Post post)
		{
			DateTime eventDate = EventDate(post);
			return String.Format("{0}?d={1}{2}", post.Category.Url, eventDate.Year.ToString().PadLeft(4, '0'), eventDate.Month.ToString().PadLeft(2, '0'));
		}

		public PostCollection UpcomingEvents()
		{
			return UpcomingEvents(-1);
		}

		public PostCollection UpcomingEvents(string tag)
		{
			return UpcomingEvents(tag, -1);
		}

		public PostCollection UpcomingEvents(int count)
		{
			return UpcomingEvents(null, count);
		}

		public PostCollection UpcomingEvents(string tag, int count)
		{
			CategoryController cc = new CategoryController();
			Category eventCategory = cc.GetCachedCategory("Events", true);

			DataBuddy.Table table = new DataBuddy.Table("graffiti_posts", "PostCollection");
			Query query = new Query(table);
			query.Top = "100 PERCENT *";

			Column categoryColumn = new Column("CategoryId", DbType.Int32, typeof(Int32), "CategoryId", false, false);
			query.AndWhere(categoryColumn, eventCategory.Id, Comparison.Equals);

			PostCollection posts = PostCollection.FetchByQuery(query);

			List<Post> eventPosts = posts
				.Where(p => p.IsInFuture())
				.Where(p => String.IsNullOrEmpty(tag) || p.TagList.Contains(tag)).ToList();

			if (String.IsNullOrEmpty(tag))
			{
				eventPosts.AddRange(Utility.LoadFeedEvents(DateTime.Today, DateTime.MaxValue).ToPosts());
			}

			eventPosts = eventPosts.OrderBy(p => p.GetEffectiveDate()).ToList();

			count = count > eventPosts.Count || count == -1 ? eventPosts.Count : count;
			eventPosts = eventPosts.GetRange(0, count);
			PostCollection ret = new PostCollection();
			ret.AddRange(eventPosts);
			return ret;
		}

		public PostCollection PastEvents()
		{
			return PastEvents(-1);
		}

		public PostCollection PastEvents(string tag)
		{
			return PastEvents(tag, -1);
		}

		public PostCollection PastEvents(int count)
		{
			return PastEvents(null, count);
		}

		public PostCollection PastEvents(string tag, int count)
		{
			CategoryController cc = new CategoryController();
			Category eventCategory = cc.GetCachedCategory("Events", true);

			DataBuddy.Table table = new DataBuddy.Table("graffiti_posts", "PostCollection");
			Query query = new Query(table);
			query.Top = "100 PERCENT *";

			Column categoryColumn = new Column("CategoryId", DbType.Int32, typeof(Int32), "CategoryId", false, false);
			query.AndWhere(categoryColumn, eventCategory.Id, Comparison.Equals);

			PostCollection posts = PostCollection.FetchByQuery(query);

			List<Post> eventPosts = posts
				.Where(p => p.IsInPast())
				.Where(p => String.IsNullOrEmpty(tag) || p.TagList.Contains(tag)).ToList();

			if (String.IsNullOrEmpty(tag))
			{
				eventPosts.AddRange(Utility.LoadFeedEvents(DateTime.MinValue, DateTime.Today.AddDays(1)).ToPosts());
			}

			eventPosts = eventPosts.OrderByDescending(p => p.GetEffectiveDate()).ToList();

			count = count > eventPosts.Count || count == -1 ? eventPosts.Count : count;
			eventPosts = eventPosts.GetRange(0, count);
			PostCollection ret = new PostCollection();
			ret.AddRange(eventPosts);
			return ret;
		}

		public string MultipleMiniCalendars(int count, bool startWithNextMonth)
		{
			int year = 0;
			int month = 0;

			CalendarFunctions.GetMonth(ref year, ref month);

			if (startWithNextMonth)
			{
				CalendarFunctions.IncrementMonth(ref year, ref month);
			}

			string calendars = "";

			for (int i = 0; i < count; ++i)
			{
				calendars += MiniCalendar(year, month);
				CalendarFunctions.IncrementMonth(ref year, ref month);
			}

			return calendars;
		}

		public string MiniCalendar()
		{
			int year = 0;
			int month = 0;

			CalendarFunctions.GetMonth(ref year, ref month);

			return MiniCalendar(year, month);
		}

		private string MiniCalendar(int year, int month)
		{
			DateTime currentMonth = new DateTime(year, month, 1);

			string ret = CalendarFunctions.BuildCalendar(false, year, month, null);
			return ret;
		}

		public string MiniCalendar(Post post)
		{
			DateTime eventDate = post.GetEventDate();
			if (eventDate != DateTime.MinValue)
			{
				return CalendarFunctions.BuildCalendar(false, eventDate.Year, eventDate.Month, post);
			}
			else
			{
				DateTime startDate = post.GetStartDate();
				DateTime endDate = post.GetEndDate();

				StringBuilder sb = new StringBuilder();
				int year = startDate.Year;
				int month = startDate.Month;

				do
				{
					sb.AppendLine(CalendarFunctions.BuildCalendar(false, year, month, post));
					++month;
					if (month == 13)
					{
						month = 1;
						++year;
					}
				} while (year < endDate.Year || (year >= endDate.Year && month < endDate.Month));

				return sb.ToString();
			}
		}

		public string Calendar()
		{
			int year = 0;
			int month = 0;

			CalendarFunctions.GetMonth(ref year, ref month);

			DateTime currentMonth = new DateTime(year, month, 1);
			DateTime previousMonth = currentMonth.AddMonths(-1);
			DateTime nextMonth = currentMonth.AddMonths(1);

			string ret = String.Format("<a class=\"previousMonth\" href=\"?d={0}{1}\">{2}</a>", previousMonth.Year.ToString().PadLeft(4, '0'), previousMonth.Month.ToString().PadLeft(2, '0'), previousMonth.ToString("MMMM yyyy"));
			ret += String.Format("<a class=\"nextMonth\" href=\"?d={0}{1}\">{2}</a>", nextMonth.Year.ToString().PadLeft(4, '0'), nextMonth.Month.ToString().PadLeft(2, '0'), nextMonth.ToString("MMMM yyyy"));
			ret += CalendarFunctions.BuildCalendar(true, year, month, null);

			return ret;
		}

		public string ExternalEventLink(Post post)
		{
			return RenderControl(Utility.ExternalEventLink(post));
		}

		private string RenderControl(Control ctrl)
		{
			StringBuilder sb = new StringBuilder();
			StringWriter tw = new StringWriter(sb);
			HtmlTextWriter hw = new HtmlTextWriter(tw);

			ctrl.RenderControl(hw);
			return sb.ToString();
		}
	}
}

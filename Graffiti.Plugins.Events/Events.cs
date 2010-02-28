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
using DataBuddy;
using Graffiti.Core;

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
			return CalendarFunctions.TryDateTimeParse(post.CustomFields()["Event Date"]);
		}

		public string PostMonthUrl(Post post)
		{
			DateTime eventDate = EventDate(post);
			return String.Format("{0}?d={1}{2}", post.Category.Url, eventDate.Year.ToString().PadLeft(4, '0'), eventDate.Month.ToString().PadLeft(2, '0'));
		}

		public PostCollection UpcomingEvents(int count)
		{
			CategoryController cc = new CategoryController();
			Category eventCategory = cc.GetCachedCategory("Events", true);

			DataBuddy.Table table = new DataBuddy.Table("graffiti_posts", "PostCollection");
			Query query = new Query(table);
			query.Top = "100 PERCENT *";

			Column categoryColumn = new Column("CategoryId", DbType.Int32, typeof(Int32), "CategoryId", false, false);
			query.AndWhere(categoryColumn, eventCategory.Id, Comparison.Equals);

			PostCollection posts = PostCollection.FetchByQuery(query);

			List<Post> eventPosts = posts.FindAll(delegate(Post p)
			{
				DateTime postDate = DateTime.Parse(p.Custom("Event Date"));
				return postDate >= DateTime.Today;
			});

			eventPosts.Sort(delegate(Post p1, Post p2)
			{
				DateTime p1Date = DateTime.Parse(p1.Custom("Event Date"));
				DateTime p2Date = DateTime.Parse(p2.Custom("Event Date"));
				return p1Date.CompareTo(p2Date);
			});

			count = count > eventPosts.Count ? eventPosts.Count : count;
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
			DateTime eventDate = DateTime.Parse(post.Custom("Event Date"));
			return CalendarFunctions.BuildCalendar(false, eventDate.Year, eventDate.Month, post);
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
	}
}

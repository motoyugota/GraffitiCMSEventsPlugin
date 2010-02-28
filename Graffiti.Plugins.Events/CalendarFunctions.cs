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
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DataBuddy;
using Graffiti.Core;

namespace Graffiti.Plugins.Events
{
	internal static class CalendarFunctions
	{
		public static DateTime TryDateTimeParse(string dateTime)
		{
			try
			{
				return DateTime.Parse(dateTime);
			}
			catch
			{
				return DateTime.MinValue;
			}
		}

		public static bool ConvertStringToBool(string checkValue)
		{
			if (string.IsNullOrEmpty(checkValue))
				return false;
			else if (checkValue == "checked" || checkValue == "on")
				return true;
			else
				return bool.Parse(checkValue);
		}

		public static void GetMonth(ref int year, ref int month)
		{
			month = DateTime.Today.Month;
			year = DateTime.Today.Year;

			string date = HttpContext.Current.Request.QueryString["d"];
			if (date != null && date.Length == 6)
			{
				string dateYear = date.Substring(0, 4);
				string dateMonth = date.Substring(4, 2);
				int tempYear = TryIntParse(dateYear, -1);
				int tempMonth = TryIntParse(dateMonth, -1);

				if (tempYear > 0 && tempMonth > 0)
				{
					year = tempYear;
					month = tempMonth;
				}
			}
		}

		public static void IncrementMonth(ref int year, ref int month)
		{
			++month;

			if (month == 13)
			{
				month = 1;
				++year;
			}
		}

		public static string BuildCalendar(bool showEvents, int year, int month, Post eventPost)
		{
			DateTime firstOfMonth = new DateTime(year, month, 1);
			int daysInMonth = DateTime.DaysInMonth(year, month);

			List<Post> monthPosts = GetPosts(year, month);

			DayOfWeek monthStartDay = firstOfMonth.DayOfWeek;

			HtmlTable calendar = new HtmlTable();
			HtmlTableRow week = new HtmlTableRow();

			if (showEvents)
			{
				calendar.Attributes.Add("class", "calendar");
			}
			else if (eventPost != null)
			{
				calendar.Attributes.Add("class", "miniCalendar event");
			}
			else
			{
				calendar.Attributes.Add("class", "miniCalendar");
			}

			int weekIndex = 0;

			while (weekIndex < (int)monthStartDay)
			{
				AddDayContent(week, "&nbsp;");
				++weekIndex;
			}

			for (int d = 1; d <= daysInMonth; ++d)
			{
				if (weekIndex == 7)
				{
					calendar.Rows.Add(week);
					week = new HtmlTableRow();
					weekIndex = 0;
				}

				bool isEventDate = false;
				DateTime date = new DateTime(year, month, d);
				if (eventPost != null && date == DateTime.Parse(eventPost.Custom("Event Date")))
				{
					isEventDate = true;
				}

				AddDayContent(week, BuildDay(monthPosts, d, showEvents, isEventDate));
				++weekIndex;
			}

			while (weekIndex <= 6)
			{
				AddDayContent(week, "&nbsp;");
				++weekIndex;
			}

			calendar.Rows.Add(week);

			HtmlGenericControl header = new HtmlGenericControl("h2");
			header.Controls.Add(new LiteralControl(firstOfMonth.ToString("MMMM yyyy")));
			HtmlGenericControl calendarMonth = new HtmlGenericControl("div");
			calendarMonth.Attributes.Add("class", "calendarMonth");
			calendarMonth.Controls.Add(header);
			calendarMonth.Controls.Add(calendar);

			TextWriter stringWriter = new StringWriter();
			HtmlTextWriter writer = new HtmlTextWriter(stringWriter);

			calendarMonth.RenderControl(writer);
			return stringWriter.ToString();
		}

		private static HtmlTableCell BuildDay(List<Post> monthPosts, int day, bool showEvents, bool isEventDate)
		{
			HtmlTableCell dayCell = new HtmlTableCell();

			dayCell.Controls.Add(new LiteralControl("<div class=\"calendarDate\">" + day.ToString() + "</div>"));
			List<Post> dayPosts = monthPosts.FindAll(delegate(Post post)
			{
				DateTime eventDate = DateTime.Parse(post.Custom("Event Date"));
				return eventDate.Day == day;
			});

			if (isEventDate)
			{
				dayCell.Attributes.Add("class", "eventDate");
			}

			if (dayPosts.Count > 0)
			{
				if (isEventDate)
				{
					dayCell.Attributes.Add("class", "eventDate hasEvents");
				}
				else
				{
					dayCell.Attributes.Add("class", "hasEvents");
				}

				if (showEvents)
				{
					HtmlGenericControl eventList = new HtmlGenericControl("ul");
					foreach (Post post in dayPosts)
					{
						HtmlGenericControl eventListItem = new HtmlGenericControl("li");
						HyperLink eventLink = new HyperLink();
						eventLink.Text = post.Title;
						eventLink.NavigateUrl = post.Url;
						eventListItem.Controls.Add(eventLink);
						eventList.Controls.Add(eventListItem);
					}
					dayCell.Controls.Add(eventList);
				}
			}

			return dayCell;
		}

		private static void AddDayContent(HtmlTableRow week, HtmlTableCell day)
		{
			week.Cells.Add(day);
		}

		private static void AddDayContent(HtmlTableRow week, string content)
		{
			HtmlTableCell day = new HtmlTableCell();
			day.Controls.Add(new LiteralControl(content));
			week.Cells.Add(day);

		}

		private static List<Post> GetPosts(int year, int month)
		{
			CategoryController cc = new CategoryController();
			Category eventCategory = cc.GetCachedCategory("Events", true);

			DataBuddy.Table table = new DataBuddy.Table("graffiti_posts", "PostCollection");
			Query query = new Query(table);
			query.Top = "100 PERCENT *";

			Column categoryColumn = new Column("CategoryId", DbType.Int32, typeof(Int32), "CategoryId", false, false);
			query.AndWhere(categoryColumn, eventCategory.Id, Comparison.Equals);

			PostCollection posts = PostCollection.FetchByQuery(query);

			return posts.FindAll(delegate(Post post)
			{
				DateTime eventDate = DateTime.Parse(post.Custom("Event Date"));
				return eventDate.Month == month && eventDate.Year == year;
			});
		}

		private static int TryIntParse(string value, int defaultValue)
		{
			try
			{
				return int.Parse(value);
			}
			catch
			{
				return defaultValue;
			}
		}
	}
}

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
using System.Collections.Specialized;
using Graffiti.Core;

namespace Graffiti.Plugins.Events
{
	public class EventsPlugin : GraffitiEvent
	{
		private readonly string eventDateFieldName = "Event Date";
		private readonly string startTimeFieldName = "Start Time";
		private readonly string endTimeFieldName = "End Time";
		private readonly string eventCategoryName = "Events";

		public bool EnableEvents { get; set; }

		public override bool IsEditable
		{
			get { return true; }
		}

		public override string Name
		{
			get { return "Events Plugin"; }
		}

		public override string Description
		{
			get { return "Allow Scheduling Posts as Events.<br />Note that disabling this plugin does not turn off the functionality, because all this plugin does is add a category and custom fields to the site.<br />Likewise, if you save changes on the \"Edit\" screen, the plugin will make changes, even if it is disabled here."; }
		}

		public override void Init(GraffitiApplication ga)
		{
		}

		protected override FormElementCollection AddFormElements()
		{
			FormElementCollection fec = new FormElementCollection();

			fec.Add(new CheckFormElement("enableEvents", "Enable Events", "Allows you to mark posts as Events, which will then show up in an event calendar", false));

			return fec;
		}

		protected override System.Collections.Specialized.NameValueCollection DataAsNameValueCollection()
		{
			NameValueCollection nvc = new NameValueCollection();
			nvc["enableEvents"] = this.EnableEvents.ToString();
			return nvc;
		}

		public override StatusType SetValues(System.Web.HttpContext context, NameValueCollection nvc)
		{
			this.EnableEvents = CalendarFunctions.ConvertStringToBool(nvc["enableEvents"]);

			if (this.EnableEvents)
			{
				SetUpEvents();
			}

			return StatusType.Success;
		}

		private void SetUpEvents()
		{
			Category eventCategory = AddEventCategory();
			AddEventFields(eventCategory);
		}

		private void AddEventFields(Category eventCategory)
		{
			bool eventDateFieldExists = false;
			bool startTimeFieldExists = false;
			bool endTimeFieldExists = false;

			CustomFormSettings cfs = CustomFormSettings.Get(eventCategory, false);
			if (cfs.Fields != null && cfs.Fields.Count > 0)
			{
				foreach (CustomField cf in cfs.Fields)
				{
					if (!eventDateFieldExists && Util.AreEqualIgnoreCase(eventDateFieldName, cf.Name))
					{
						eventDateFieldExists = true;
					}
					if (!startTimeFieldExists && Util.AreEqualIgnoreCase(startTimeFieldName, cf.Name))
					{
						startTimeFieldExists = true;
					}
					if (!endTimeFieldExists && Util.AreEqualIgnoreCase(endTimeFieldName, cf.Name))
					{
						endTimeFieldExists = true;
					}

					if (eventDateFieldExists && startTimeFieldExists && endTimeFieldExists)
					{
						break;
					}
				}
			}

			if (!eventDateFieldExists)
			{
				CustomField dateField = new CustomField();
				dateField.Name = eventDateFieldName;
				dateField.Description = "The date that the event takes place on";
				dateField.Enabled = true;
				dateField.Id = Guid.NewGuid();
				dateField.FieldType = FieldType.DateTime;

				cfs.Name = eventCategory.Id.ToString();
				cfs.Add(dateField);
				cfs.Save();
			}

			if (!startTimeFieldExists)
			{
				CustomField startField = new CustomField();
				startField.Name = startTimeFieldName;
				startField.Description = "The time that the event starts";
				startField.Enabled = true;
				startField.Id = Guid.NewGuid();
				startField.FieldType = FieldType.TextBox;

				cfs.Name = eventCategory.Id.ToString();
				cfs.Add(startField);
				cfs.Save();
			}

			if (!endTimeFieldExists)
			{
				CustomField endField = new CustomField();
				endField.Name = endTimeFieldName;
				endField.Description = "The time that the event ends";
				endField.Enabled = true;
				endField.Id = Guid.NewGuid();
				endField.FieldType = FieldType.TextBox;

				cfs.Name = eventCategory.Id.ToString();
				cfs.Add(endField);
				cfs.Save();
			}
		}

		private Category AddEventCategory()
		{
			CategoryController cc = new CategoryController();
			Category eventCategory = cc.GetCachedCategory(this.eventCategoryName, true);
			if (eventCategory == null)
			{
				eventCategory = new Category();
				eventCategory.Name = this.eventCategoryName;
				eventCategory.ParentId = -1;
				eventCategory.Save();
			}

			return eventCategory;
		}
	}
}

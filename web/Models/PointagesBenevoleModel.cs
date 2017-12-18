﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class PointagesBenevoleModel
    {
        public const int CALENDAR_ROW_COUNT = 6;
        public const int CALENDAR_DAY_COUNT = 7;

        public DateTime MonthDate { get; set; }

        public dal.models.Benevole Benevole { get; set; }

        public List<CalendarRow> CalendarRows { get; set; }

        public PointagesBenevoleModel()
        {
            this.CalendarRows = new List<CalendarRow>();
        }
    }

    public class CalendarRow
    {
        public List<CalendarItem> Items { get; set; }

        public CalendarRow()
        {
            this.Items = new List<CalendarItem>();
        }
    }

    public class CalendarItem
    {
        public DateTime Date { get; set; }

        public dal.models.Pointage Pointage { get; set; }

        public int ColumnIndex { get; set; }

        public int RowIndex { get; set; }

        public bool IsCurrentMonth { get; set; }

        public string GetPointageCssClass()
        {
            string css = "pointage unselected";

            if (this.Pointage == null || this.Pointage.NbDemiJournees == 0)
                css += " none";
            else
            {
                if(this.Pointage.NbDemiJournees == 1)
                    css += " half";
                else
                    css += " full";
            }

            if (this.RowIndex == 0)
                css += " top";
            else if(this.RowIndex == PointagesBenevoleModel.CALENDAR_ROW_COUNT - 1)
                css += " bottom";
            else
                css += " vmiddle";

            if (this.ColumnIndex == 0)
                css += " left";
            else if (this.ColumnIndex == PointagesBenevoleModel.CALENDAR_DAY_COUNT - 1)
                css += " right";
            else
                css += " hmiddle";

            return css;
        }

        public string GetDayTextCssClass()
        {
            if (this.IsCurrentMonth)
                return "current_month";
            else
                return "other_month";
        }

        public string GetTitle()
        {
            if (this.Pointage != null)
                return $"{this.Pointage.Distance} km";
            else
                return string.Empty;
        }
    }
}


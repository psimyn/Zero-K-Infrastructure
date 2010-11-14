﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMissionLib.Conditions
{
	[DataContract]
	public class TimeCondition : Condition
	{
		TimeSpan time;

		public TimeCondition()
			: base() {}

		[DataMember]
		public TimeSpan Time
		{
			get { return time; }
			set
			{
				time = value;
				RaiseTimeChanged();
			}
		}

		public double Seconds
		{
			get { return time.TotalSeconds; }
			set
			{
				time = TimeSpan.FromSeconds(value);
				RaiseTimeChanged();
			}
		}

		public double Minutes
		{
			get { return time.TotalMinutes; }
			set
			{
				time = TimeSpan.FromMinutes(value);
				RaiseTimeChanged();
			}
		}


		public double Frames // 30 gameframes per second
		{
			get { return time.TotalSeconds*30; }
			set
			{
				time = TimeSpan.FromSeconds(value/30);
				RaiseTimeChanged();
			}
		}

		public override LuaTable GetLuaTable(Mission mission)
		{
			var map = new Dictionary<object, object>
				{
					{"frames", Math.Floor(Frames)},
				};
			return new LuaTable(map);
		}

		public override string GetDefaultName()
		{
			return "Metronome Ticks";
		}

		void RaiseTimeChanged()
		{
			RaisePropertyChanged("Seconds");
			RaisePropertyChanged("Time");
			RaisePropertyChanged("Minutes");
			RaisePropertyChanged("Frames");
		}
	}
}
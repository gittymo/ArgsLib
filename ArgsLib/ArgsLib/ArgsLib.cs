using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArgsLib
{
	sealed public class Argument
	{

		public Argument(string[] args, char charId)
		{
			init(args, "" + charId, null, -1, -1);
		}

		public Argument(string[] args, string stringId)
		{
			init(args, null, stringId, -1, -1);
		}

		public Argument(string[] args, char charId, int minimumRequiredValues)
		{
			init(args, "" + charId, null, minimumRequiredValues, -1);
		}

		public Argument(string[] args, string stringId, int minimumRequiredValues)
		{
			init(args, null, stringId, minimumRequiredValues, -1);
		}

		public Argument(string[] args, char charId, int minimumRequiredValues, int maximumRequiredValues)
		{
			init(args, "" + charId, null, minimumRequiredValues, maximumRequiredValues);
		}

		public Argument(string[] args, string stringId, int minimumRequiredValues, int maximumRequiredValues)
		{
			init(args, null, stringId, minimumRequiredValues, maximumRequiredValues);
		}

		public Argument(string[] args, char charId, string stringId, int minimumRequiredValues, int maximumRequiredValues)
		{
			init(args, "" + charId, stringId, minimumRequiredValues, maximumRequiredValues);
		}

		public Argument(string[] args, char charId, bool valueRequired)
		{
			init(args, "" + charId, null, (valueRequired) ? 1 : -1, -1);
		}

		public Argument(string[] args, string stringId, bool valueRequired)
		{
			init(args, null, stringId, (valueRequired) ? 1 : -1, -1);
		}

		private void init(string[] args, string charId, string stringId, int minValueCountIn, int maxValueCountIn)
		{
			this.charId = charId;
			this.stringId = stringId;
			this.provided = false;
			this.currentValueIndex = 0;

			if (maxValueCountIn < 0 || maxValueCountIn > 8) maxValueCountIn = 8;
			if (minValueCountIn > maxValueCountIn) this.minValueCount = this.maxValueCount;
			else this.minValueCount = minValueCountIn;
			this.maxValueCount = maxValueCountIn;

			if (args != null && args.Length > 0 && ((charId != null && charId.Length == 1) || (stringId != null && stringId.Length >= 1)))
			{
				values = new List<string>();
				bool allowAddValue = false;
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].IndexOf('-') == 0)
					{
						string sensibleArgTag = CreateSensibleArgumentTag(args[i]);
						if (sensibleArgTag.Length > 1) allowAddValue = ArgIsMatch(sensibleArgTag);
						if (allowAddValue) provided = true;
					}
					else
					{
						if ((values.Count < this.maxValueCount || values.Count < this.minValueCount) && allowAddValue) values.Add(args[i]);
					}
				}
				if (values.Count < this.minValueCount) throw new InsufficientArgumentValuesException(this);
			}
		}

		public static string CreateSensibleArgumentTag(string tagString)
		{
			string tidyString = null;
			if (tagString != null && tagString.Length > 0)
			{
				Regex removeRubbishRegex = new Regex("[^A-Za-z0-9-_]");
				Regex sanityCheckRegex = new Regex("-*[A-Za-z0-9][A-Za-z0-9-_]*");
				string cleanTagString = removeRubbishRegex.Replace(tagString, "");
				char[] cleanTagStringChars = cleanTagString.ToCharArray();
				char[] tidyTagStringChars = new char[cleanTagStringChars.Length];
				int j = 0;
				int consecutiveHypensFound = 0;
				char lastNonHyphenChar = '\0';
				for (int i = 0; i < cleanTagStringChars.Length; i++)
				{
					if (cleanTagStringChars[i] == '-')
					{
						if (consecutiveHypensFound < 1 && lastNonHyphenChar != '_' && consecutiveHypensFound != i) tidyTagStringChars[j++] = '-';
						consecutiveHypensFound++;
					}
					else
					{
						if (consecutiveHypensFound > 0) consecutiveHypensFound = 0;
						tidyTagStringChars[j++] = cleanTagStringChars[i];
						lastNonHyphenChar = cleanTagStringChars[i];
					}
					tidyString = removeRubbishRegex.Replace(new string(tidyTagStringChars), "");
					tidyString = sanityCheckRegex.IsMatch(tidyString) ? tidyString : null;
				}
			}

			return tidyString;
		}

		public bool IsProvided => provided;

		public bool HasValues => values.Count > 0;

		public List<string> Values => values.ToList();

		public string FirstValue()
		{
			currentValueIndex = 0;
			return GetValue();
		}

		public string GetValue()
		{
			if (values.Count > 0 && currentValueIndex < values.Count) return values[currentValueIndex++];
			return null;
		}

		public string GetMessageName()
		{
			if (charId != null)
			{
				if (stringId != null) return charId + "( or " + stringId + " )";
				else return charId;
			}
			else return stringId;
		}

		private bool ArgIsMatch(string argName) => ((charId != null && argName.Equals(charId)) ||
								(stringId != null && argName.Equals(stringId, StringComparison.CurrentCultureIgnoreCase)));

		private string charId;
		private string stringId;
		private bool provided;
		internal int minValueCount, maxValueCount;
		internal List<string> values;
		private int currentValueIndex;
	}

	sealed public class InsufficientArgumentValuesException : Exception
	{
		public InsufficientArgumentValuesException(Argument arg) :
			base("Only " + arg.values.Count + " of " + arg.minValueCount + " values provided for argument " + arg.GetMessageName())
		{ }
	}
}
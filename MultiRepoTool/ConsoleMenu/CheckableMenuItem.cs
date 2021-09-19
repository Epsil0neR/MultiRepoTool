namespace MultiRepoTool.ConsoleMenu
{
	public class CheckableMenuItem<T> : MenuItem
	{
		private readonly string _title;
		private bool _isChecked;

		public bool IsChecked
		{
			get => _isChecked;
			set
			{
				if (_isChecked == value)
					return;

				_isChecked = value;
				Title = (IsChecked ? "+ " : "  ") + _title;
			}
		}

		public T Value { get; }

		public CheckableMenuItem(string title, T value)
			: base(string.Empty)
		{
			_title = title;
			Title = (IsChecked ? "+ " : "  ") + _title;

			Value = value;
		}

		public override bool Execute(Menu menu)
		{
			IsChecked = !IsChecked;
			return true;
		}
	}
}
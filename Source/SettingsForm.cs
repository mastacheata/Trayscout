using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Trayscout
{
    public class SettingsForm : Form
    {
        private const string Section = "Config";

        private readonly Ini _ini;
        private readonly TableLayoutPanel _table;
        private int _row;

        private TextBox _baseUrl;
        private TextBox _accessToken;
        private TextBox _apiSecret;
        private CheckBox _showSecrets;
        private NumericUpDown _updateInterval;
        private TextBox _high;
        private TextBox _low;
        private ComboBox _unit;
        private CheckBox _useColor;
        private CheckBox _useAlarm;
        private NumericUpDown _alarmInterval;
        private ComboBox _style;
        private NumericUpDown _timeRange;
        private NumericUpDown _width;
        private NumericUpDown _height;
        private ComboBox _fontFamily;
        private NumericUpDown _fontSize;

        public SettingsForm(Ini ini)
        {
            _ini = ini;

            Text = "Trayscout Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(10);

            _table = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                Dock = DockStyle.Fill
            };

            _baseUrl = new TextBox();
            AddRow("Base URL", _baseUrl);
            _accessToken = new TextBox { UseSystemPasswordChar = true };
            AddRow("Access token", _accessToken);
            _apiSecret = new TextBox { UseSystemPasswordChar = true };
            AddRow("API secret", _apiSecret);
            _showSecrets = new CheckBox { Text = "Show token / secret", AutoSize = true };
            _showSecrets.CheckedChanged += (s, e) =>
            {
                _accessToken.UseSystemPasswordChar = !_showSecrets.Checked;
                _apiSecret.UseSystemPasswordChar = !_showSecrets.Checked;
            };
            AddRow("", _showSecrets);
            _updateInterval = Numeric(1, 60);
            AddRow("Update interval (min)", _updateInterval);
            _high = new TextBox();
            AddRow("High", _high);
            _low = new TextBox();
            AddRow("Low", _low);
            _unit = Combo(Enum.GetNames(typeof(Unit)), true);
            AddRow("Unit", _unit);
            _useColor = new CheckBox { Text = "Use colors", AutoSize = true };
            AddRow("", _useColor);
            _useAlarm = new CheckBox { Text = "Play alarm when out of range", AutoSize = true };
            AddRow("", _useAlarm);
            _alarmInterval = Numeric(1, 1440);
            AddRow("Alarm interval (min)", _alarmInterval);
            _style = Combo(Enum.GetNames(typeof(StyleKey)), true);
            AddRow("Style", _style);
            _timeRange = Numeric(1, 6);
            AddRow("Time range (h)", _timeRange);
            _width = Numeric(100, 2000);
            AddRow("Diagram width (px)", _width);
            _height = Numeric(100, 2000);
            AddRow("Diagram height (px)", _height);
            _fontFamily = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
            foreach (FontFamily family in FontFamily.Families)
                _fontFamily.Items.Add(family.Name);
            AddRow("Font family", _fontFamily);
            _fontSize = Numeric(4, 72);
            AddRow("Font size", _fontSize);

            Button ok = new Button { Text = "Save", DialogResult = DialogResult.None, AutoSize = true };
            ok.Click += OnSave;
            Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            AddRow("", buttons);

            AcceptButton = ok;
            CancelButton = cancel;
            Controls.Add(_table);

            LoadValues();
        }

        private void AddRow(string label, Control control)
        {
            _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _table.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 12, 6) }, 0, _row);
            if (control is TextBox || control is ComboBox || control is NumericUpDown)
                control.Width = 240;
            control.Margin = new Padding(3, 3, 3, 3);
            _table.Controls.Add(control, 1, _row);
            _row++;
        }

        private static NumericUpDown Numeric(int min, int max)
        {
            return new NumericUpDown { Minimum = min, Maximum = max };
        }

        private static ComboBox Combo(string[] items, bool dropDownList)
        {
            ComboBox box = new ComboBox { DropDownStyle = dropDownList ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown };
            box.Items.AddRange(items);
            return box;
        }

        private string Read(string key)
        {
            return _ini.ReadOptionalString(Section, key) ?? "";
        }

        private void SetNumeric(NumericUpDown control, string key)
        {
            if (int.TryParse(Read(key), out int value))
                control.Value = Math.Max(control.Minimum, Math.Min(control.Maximum, value));
            else
                control.Value = control.Minimum;
        }

        private void SetCombo(ComboBox control, string key)
        {
            string value = Read(key);
            int index = control.FindStringExact(value);
            if (index >= 0)
                control.SelectedIndex = index;
            else if (control.DropDownStyle == ComboBoxStyle.DropDown)
                control.Text = value;
            else if (control.Items.Count > 0)
                control.SelectedIndex = 0;
        }

        private void LoadValues()
        {
            _baseUrl.Text = Read("BaseUrl");
            _accessToken.Text = Read("AccessToken");
            _apiSecret.Text = Read("APISecret");
            SetNumeric(_updateInterval, "UpdateInterval");
            _high.Text = Read("High");
            _low.Text = Read("Low");
            SetCombo(_unit, "Unit");
            _useColor.Checked = Read("UseColor") == "1";
            _useAlarm.Checked = Read("UseAlarm") == "1";
            SetNumeric(_alarmInterval, "AlarmInterval");
            SetCombo(_style, "Style");
            SetNumeric(_timeRange, "TimeRange");
            SetNumeric(_width, "Width");
            SetNumeric(_height, "Height");
            SetCombo(_fontFamily, "FontFamily");
            SetNumeric(_fontSize, "FontSize");
        }

        private static bool TryParseFloat(string text, out float value)
        {
            return float.TryParse(text.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
        }

        private string Validate()
        {
            if (string.IsNullOrWhiteSpace(_baseUrl.Text))
                return "Base URL is required.";
            if (string.IsNullOrWhiteSpace(_accessToken.Text) && string.IsNullOrWhiteSpace(_apiSecret.Text))
                return "Either an access token or an API secret is required.";
            if (!TryParseFloat(_high.Text, out float high))
                return "High is not a number.";
            if (!TryParseFloat(_low.Text, out float low))
                return "Low is not a number.";
            if (low >= high)
                return "Low must be smaller than High.";
            if (string.IsNullOrWhiteSpace(_fontFamily.Text))
                return "Font family is required.";
            return null;
        }

        private void OnSave(object sender, EventArgs e)
        {
            string error = Validate();
            if (error != null)
            {
                MessageBox.Show(this, error, "Invalid settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                TryParseFloat(_high.Text, out float high);
                TryParseFloat(_low.Text, out float low);

                _ini.WriteString(Section, "BaseUrl", _baseUrl.Text.Trim());
                _ini.WriteString(Section, "AccessToken", _accessToken.Text.Trim());
                _ini.WriteString(Section, "APISecret", _apiSecret.Text.Trim());
                _ini.WriteString(Section, "UpdateInterval", ((int)_updateInterval.Value).ToString(CultureInfo.InvariantCulture));
                _ini.WriteString(Section, "High", high.ToString(CultureInfo.InvariantCulture));
                _ini.WriteString(Section, "Low", low.ToString(CultureInfo.InvariantCulture));
                _ini.WriteString(Section, "Unit", _unit.Text);
                _ini.WriteString(Section, "UseColor", _useColor.Checked ? "1" : "0");
                _ini.WriteString(Section, "UseAlarm", _useAlarm.Checked ? "1" : "0");
                _ini.WriteString(Section, "AlarmInterval", ((int)_alarmInterval.Value).ToString(CultureInfo.InvariantCulture));
                _ini.WriteString(Section, "Style", _style.Text);
                _ini.WriteString(Section, "TimeRange", ((int)_timeRange.Value).ToString(CultureInfo.InvariantCulture));
                _ini.WriteString(Section, "Width", ((int)_width.Value).ToString(CultureInfo.InvariantCulture));
                _ini.WriteString(Section, "Height", ((int)_height.Value).ToString(CultureInfo.InvariantCulture));
                _ini.WriteString(Section, "FontFamily", _fontFamily.Text.Trim());
                _ini.WriteString(Section, "FontSize", ((int)_fontSize.Value).ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Could not save settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UPPPDGenerator.Managers
{
    public class FieldValidatorManager
    {
        public List<(Control, int)> CheckFields(List<Control> controls)
        {
            List<(Control, int)> listofcontrols = new List<(Control, int)>();
            foreach (Control control in controls)
            {
                if (control is TextBox textBox)
                {
                    bool isTrue = ValidateTextBox(textBox);
                    if (!isTrue) listofcontrols.Add((textBox, 0));
                    else listofcontrols.Add((textBox, 1));
                }
                if (control is PasswordBox passwordBox) 
                {
                    bool isTrue = ValidatePasswordBox(passwordBox);
                    if (!isTrue) listofcontrols.Add((passwordBox, 0));
                    else listofcontrols.Add ((passwordBox, 1));
                }
            }
            return listofcontrols;
        }
        public bool ValidateTextBox(TextBox textBox)
        {
            if (textBox.Name != "remail")
            {
                bool isTrue = true;
                if (textBox.Text == "")
                {
                    isTrue = false;
                }
                else if (string.IsNullOrEmpty(textBox.Text))
                {
                    isTrue = false;
                }
                else if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    isTrue = false;
                }
                return isTrue;
            }
            return false;
        }
        public bool ValidatePasswordBox(PasswordBox passwordBox)
        {
            if (passwordBox.Name != "pass")
            {
                bool isTrue = true;
                if (passwordBox.Password == "")
                {
                    isTrue = false;
                }
                else if (string.IsNullOrEmpty(passwordBox.Password))
                {
                    isTrue = false;
                }
                else if (string.IsNullOrWhiteSpace(passwordBox.Password))
                {
                    isTrue = false;
                }
                return isTrue;
            }
            return false;
        }
    }
}

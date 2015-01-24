using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CryptoManager
{
    /// <summary>
    /// Interaction logic for PasswordMeter.xaml
    /// </summary>
    public partial class PasswordMeter : UserControl
    {
        public const int UNKNOWN = 0;
        public const int VERY_WEAK = 1;
        public const int WEAK = 2;
        public const int GOOD = 3;
        public const int STRONG = 4;
        public const int VERY_STRONG = 5;
        
        private PasswordStrength m_pwdStrength;
        private int m_complexityRating;
        private SolidColorBrush m_solidColorBrush;

        public int Complexity
        {
            get { return m_complexityRating; }
            set { m_complexityRating = value; }
        }

        public PasswordMeter()
        {
            InitializeComponent();

            m_pwdStrength = new PasswordStrength();
            m_pwdStrength.SetPassword(" ");

            m_solidColorBrush = new SolidColorBrush();
            UpdateControl();
        }

        public void SetPassword(String pwd)
        {
            m_pwdStrength.SetPassword(pwd);
            UpdateControl();
        }

        private void UpdateControl()
        {
            m_pwdStrength.CheckPassword();
            canvas.Children.Clear();

            switch (m_pwdStrength.Complexity)
            {
                case "Very Weak":
                    m_complexityRating = VERY_WEAK;
                    m_solidColorBrush.Color = Colors.Red;
                    break;

                case "Weak":
                    m_complexityRating = WEAK;
                    m_solidColorBrush.Color = Colors.Orange;
                    break;

                case "Good":
                    m_complexityRating = GOOD;
                    m_solidColorBrush.Color = Colors.Yellow;
                    break;

                case "Strong":
                    m_complexityRating = STRONG;
                    m_solidColorBrush.Color = Colors.Green;
                    break;

                case "Very Strong":
                    m_complexityRating = VERY_STRONG;
                    m_solidColorBrush.Color = Colors.Green;
                    break;

                default:
                    m_complexityRating = UNKNOWN;
                    break;

            }

           
            for (int rank = 0; rank < m_complexityRating; rank++)
            {
                Rectangle rankRectangle = new Rectangle();
                
                rankRectangle.Height = 20;
                rankRectangle.Width = 20;

                canvas.Children.Add(rankRectangle);
                Canvas.SetTop(rankRectangle, 0);
                Canvas.SetLeft(rankRectangle, 20*rank + 40);

                rankRectangle.Fill = m_solidColorBrush;
            }

            
            
        }
    }
}

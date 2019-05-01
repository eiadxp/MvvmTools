using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xaml;
namespace MvvmTools.WPF
{
    [MarkupExtensionReturnType(typeof(object))]
    public class DesignInstance : MarkupExtension
    {
        public Type Type { get; set; }
        public bool IsCreateList { get; set; }
        public int ItemsCount { get; set; } = 3;
        public int Depth { get; set; } = 2;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Type == null) throw new ArgumentNullException(nameof(Type));
            try
            {
                return Design.Data.Create(Type, Depth, IsCreateList, ItemsCount);
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}

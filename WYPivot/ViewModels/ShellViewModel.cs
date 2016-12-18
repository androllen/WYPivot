/********************************************************************************
** 作者： Androllen
** 日期： 16/12/18 16:55:38
** 微博： http://weibo.com/Androllen
*********************************************************************************/
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYPivot.ViewModels
{
    public class ShellViewModel : Screen
    {
        public BindableCollection<string> ItemsSources
        {
            get;
            private set;
        }

       public ShellViewModel()
        {
            ItemsSources = new BindableCollection<string>();
            ItemsSources.Add("军事");
            ItemsSources.Add("科技");
            ItemsSources.Add("国内新闻");
            ItemsSources.Add("娱乐");
            ItemsSources.Add("国际新闻");
            ItemsSources.Add("相声");
            ItemsSources.Add("体育赛事");
            ItemsSources.Add("相声综艺");
        }
    }
}

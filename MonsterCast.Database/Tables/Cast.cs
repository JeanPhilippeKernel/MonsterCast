using SQLite;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterCast.Database.Tables
{
    public class Cast
    {      
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Index { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Summary { get; set; }
        public string Address { get; set; }
        public string Song { get; set; }

        [Ignore]
        public BitmapImage Cover { get; set; }
    }
}

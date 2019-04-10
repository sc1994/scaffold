using scaffold.Model;
using System.Collections.Generic;
using Xunit;

namespace XUnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var code = new CodeModel
            {
                ProjectName = "Test1",
                CheckedTables = new List<string>
                {
                    "TCDAILYSURPRISE_TEST.AIBSuspectSubject",
                    "TCDAILYSURPRISE_TEST.AIBGameInviteRecord"
                },
                Types = new List<string> { "Model" }
            };

            code.SaveModel();
            code.SaveDatabase();
            code.SaveService();
        }
    }
}

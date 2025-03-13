using Entities.DB;
using Entities.DB.Entities;

namespace UnitTests.DB;

[TestClass]
public class FakeDataTests : AbstractTest
{
    [TestMethod]
    public async Task GenerateFakeDataTests()
    {
        var user = _db.Users.FirstOrDefault();
        if (user == null) user = new User
        {
            UserPrincipalName = "UnitTestUser",
        };



        await FakeDataGen.GenerateFakeCopilotFor(user.UserPrincipalName, _db, _logger);
        await _db.SaveChangesAsync();
        await FakeDataGen.GenerateFakeOfficeActivityFor(user.UserPrincipalName, DateTime.UtcNow, _db, _logger);
        await _db.SaveChangesAsync();
    }

}

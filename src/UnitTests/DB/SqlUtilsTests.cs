using Common.DataUtils.Sql;
using Common.DataUtils.Sql.Inserts;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.DB;

[TestClass]
public class SqlUtilsTests : AbstractTest
{
    const string TABLE_NAME = "tmp_SqlUtilsTests";
    const string TEMP_TABLE_NAME = "##tmp_SqlUtilsTests";
    [TestMethod]
    public async Task InsertBatchTests()
    {
        // Normal table
        var batch = new InsertBatch<TestMultiPropTypeTempEntity>(_config.ConnectionStrings.SQL, _logger);

        // Verify prop cache
        Assert.IsTrue(batch.PropCache.PropertyMappingInfo.Count > 0);

        // Save empty
        var emptyResult = await batch.SaveToStagingTable(string.Empty);
        Assert.IsTrue(emptyResult == 0);

        // Save with data
        batch.Rows.Add(new TestMultiPropTypeTempEntity());// Use class defaults
        batch.Rows.Add(new TestMultiPropTypeTempEntity { NullableIntProp = 1 });
        await batch.SaveToStagingTable($"select * from {TABLE_NAME}");

        // Verify saved data
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();

        var selectCmd = conn.CreateCommand();
        selectCmd.CommandText = $"select * from {TABLE_NAME}";
        var results = await selectCmd.ExecuteReaderAsync();

        // x2 records
        int count = 0;
        while (results.Read())
        {
            // Recheck each property
            foreach (var pi in batch.PropCache.PropertyMappingInfo)
            {
                var sqlFieldResult = results[pi.SqlInfo.FieldName];
                Assert.IsNotNull(sqlFieldResult);

                var objectVal = pi.Property.GetValue(batch.Rows[count]);
                if (sqlFieldResult is double)
                {
                    if (pi.Property.PropertyType == typeof(double))
                    {
                        Assert.AreEqual(sqlFieldResult, objectVal);
                    }
                    else if (pi.Property.PropertyType == typeof(float))
                    {
                        // SQL reader interprets SQL "float" fields as doubles. If the original prop is a float, the double read from SQL will not match unless rounded back to float
                        Assert.AreEqual(Convert.ToSingle(sqlFieldResult), objectVal);
                    }
                    else
                    {
                        throw new NotSupportedException("Unknown property type");
                    }
                }
                else if (sqlFieldResult is DBNull)
                {
                    // Nulls are never read back as literal nulls...
                    Assert.IsTrue(objectVal == null);
                }
                else
                {
                    Assert.AreEqual(sqlFieldResult, objectVal);
                }
            }
            count++;
        }

        Assert.IsTrue(count == batch.Rows.Count);
        conn.Close();

        // Temp table tests
        var batchTemp = new InsertBatch<TestTempEntityTempTable>(_config.ConnectionStrings.SQL, _logger);

        // Save empty
        await batchTemp.SaveToStagingTable(string.Empty);

        // Save with data over several threads
        for (int i = 0; i < 105; i++)
        {
            batchTemp.Rows.Add(new TestTempEntityTempTable { Prop = "whatever " + i });
        }
        var tempUpdates = await batchTemp.SaveToStagingTable(10, $"delete from {TEMP_TABLE_NAME}");

        // Can't verify saved data as temp table will be closed
        Assert.IsTrue(tempUpdates == batchTemp.Rows.Count);

    }
    [TempTableName(TABLE_NAME)]
    class TestMultiPropTypeTempEntity
    {
        [Column("guid")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("prop")]
        public string Prop { get; set; } = "Whatever";

        /// <summary>
        /// Specifically override the nullability of this property
        /// </summary>
        [Column("nullstringprop", true)]
        public string NullProp { get; set; } = null!;

        [Column("intprop")]
        public int IntProp { get; set; } = 1;

        /// <summary>
        /// This should automatically be nullable without needing to specify it in the attribute
        /// </summary>
        [Column("nullable_intprop")]
        public int? NullableIntProp { get; set; } = null;

        [Column("floatprop")]
        public float FloatProp { get; set; } = 1.001f;

        [Column("doubleprop")]
        public double DoubleProp { get; set; } = -1.001f;

        [Column("boolprop")]
        public bool BoolProp { get; set; } = true;

        [Column("prop2", ColationOverride = "SQL_Latin1_General_CP1_CS_AS")]
        public string CaseSensitiveProp { get; set; } = "OhHey";

        [Column("date")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    [TempTableName(TEMP_TABLE_NAME)]
    class TestTempEntityTempTable
    {
        [Column("prop")]
        public string Prop { get; set; } = null!;
    }
}

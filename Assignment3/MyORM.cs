using Assignment3Test.TestCase1;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assignment3
{
    public enum DBOperationType
    {
        Insert, Update, Delete
    }
    public class MyORM<G,T>
    {
        private readonly string _connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=AspnetB9;User ID=aspnetb9; Password=123456;TrustServerCertificate=True;";
        public MyORM(string connectionString)
        {
            _connectionString = connectionString;
        }
        public MyORM() { }
        public void Insert(T item)
        {
            DBOperation(item, DBOperationType.Insert);
        }
        public void Update(T item)
        {
            DBOperation(item, DBOperationType.Update);
        }
        public void Delete(T item)
        {
            DBOperation(item, DBOperationType.Delete);
        }
        public void Delete(G id)
        {
            PerformDeletetionById(id);
        }
        public T GetById(G id)
        {
            var itemType = typeof(T);
            return PerformGetById(id, itemType);
        }
        public IEnumerable<T> GetAll()
        {
            var itemType = typeof(T);
            return PerformGetAll(itemType);
        }

        // ------------------------ Operation -------------------------------------
        public void DBOperation(T item, DBOperationType operationType)
        {
            List<object> subItems = new List<object>();
            if (item is null)
            {
                throw new ArgumentNullException("item is null");
            }
            else
            {
                if (TableExistance(typeof(T)))
                {
                    //Console.WriteLine("Table Exist");
                    var type = item.GetType();
                    var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    List<PropertyInfo> propertiesList = new List<PropertyInfo>();
                    foreach (var property in properties)
                    {
                        var propertyType = property.PropertyType;
                        if (IsIterableType(propertyType) || IsComplexType(property.PropertyType))
                        {
                            object propertyValue = property.GetValue(item);
                            if (propertyValue is not null)
                            {
                                subItems.Add(propertyValue);
                            }
                        }
                        else
                        {
                            propertiesList.Add(property);
                        }
                    }
                    PropertyInfo[] primitiveProperties = propertiesList.ToArray();
                    if (operationType == DBOperationType.Insert)
                    {
                        PerformInsertion(primitiveProperties, item);
                    }
                    if (operationType == DBOperationType.Delete)
                    {
                        PerformDeletetion(item);
                    }
                    if (operationType == DBOperationType.Update)
                    {
                        PerformUpdate(primitiveProperties, item);
                    }
                    if (subItems.Count > 0)
                    {
                        SubItemsDBOperation(subItems, item, operationType);
                        subItems.Clear();
                    }
                }
            }
        }
        // ----------------------- Insert Operation --------------------------------
        // -------------------   Insertion of Non-Primitive Type   ---------------------

        //---------------- Re exploration phase ---------------
        public void NestedTypeDBOperation(object item, Type type, object parent, Type parentType, DBOperationType operationType)
        {
            List<object> subItems = new List<object>();
            if (item is null)
            {
                throw new ArgumentNullException("item is null");
            }
            if (IsIterableType(type)) //if  iterable type is detected 
            {
                var elements = (IEnumerable)item;
                foreach (var element in elements)
                {
                    // Recursive call for each of the complex
                    NestedTypeDBOperation(element, element.GetType(), parent, parentType, operationType);
                }
            }
            else
            {
                if (SubTableExistance(item, type, parent, parentType))
                {
                    //Console.WriteLine("Sub Table Exist");
                    var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public );
                    List<PropertyInfo> propertiesList = new List<PropertyInfo>();
                    foreach (var property in properties)
                    {
                        var propertyType = property.PropertyType;
                        if (IsIterableType(propertyType) || IsComplexType(property.PropertyType))
                        {
                            object propertyValue = property.GetValue(item);
                            if (propertyValue is not null)
                            {
                                subItems.Add(propertyValue);
                            }
                        }
                        else
                        {
                            propertiesList.Add(property);
                        }
                    }
                    PropertyInfo[] primitiveProperties = propertiesList.ToArray();
                    if (operationType == DBOperationType.Insert)
                    {
                        string foreignKeyColumn = GetForiegnKey(parent);
                        G foreignKeyValue = GetValueOfId(parent);
                        PerformInsertionOfChildTable(primitiveProperties, item, foreignKeyColumn, foreignKeyValue);
                    }
                    if (operationType == DBOperationType.Update)
                    {
                        string foreignKeyColumn = GetForiegnKey(parent);
                        G foreignKeyValue = GetValueOfId(parent);
                        PerformUpdateOfChildTable(primitiveProperties, item, foreignKeyColumn, foreignKeyValue);
                    }
                    if (operationType == DBOperationType.Delete)
                    {
                        string foreignKeyColumn = GetForiegnKey(parent);
                        G foreignKeyValue = GetValueOfId(parent);
                        PerformDeletetionOfChild(item, foreignKeyColumn, foreignKeyValue);
                    }
                    if (subItems.Count > 0)
                    {
                        SubItemsDBOperation(subItems, item, operationType);
                        subItems.Clear();
                    }
                }
            }
        }
        // Operation for all of the subitems, handling through List/ dynamic array
        public void SubItemsDBOperation(List<object> items, object parent, DBOperationType operationType)
        {
            foreach (var item in items)
            {
                var type = item.GetType();
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                if (IsComplexType(type) || IsIterableType(type))
                {
                    NestedTypeDBOperation(item, type, parent, parent.GetType(), operationType);
                }
            }
        }
        //---------------- Re exploration phase End ----------------------


        // ----------------------- Insert Query --------------------------------
        public void PerformInsertion(PropertyInfo[] instances, object item)
        {
            ExecuteInsertQuery(instances, item);
        }
        public void PerformInsertionOfChildTable(PropertyInfo[] instances, object item, string foreignKeyColumn, G parentId)
        {
            ExecuteInsertQueryWithParentId(instances, item, foreignKeyColumn, parentId);
        }
        public void ExecuteInsertQuery(PropertyInfo[] instances, object item)
        {
            string tableName = GetTableName(item);
            string query = $"INSERT INTO {tableName} (";
            var (NamesOfColumns, placeHoldersOfColumn, valuesOfColumns) = FillColumnInfoInQuery(instances, item);
            List<string> columnNames = NamesOfColumns;
            List<string> columnPlaceHolders = placeHoldersOfColumn;
            List<object> values = valuesOfColumns;
            // Adding column names in query
            query = AddColumnNamesInQuery(query, columnNames);
            query = $" {query}) VALUES (";
            // Adding column placeholder in query
            query = AddColumnPlaceHolderInQuery(query, columnPlaceHolders);
            query = $" {query});";
            ExecuteSQLParameterized(query, columnPlaceHolders, values);
        }
        public void ExecuteInsertQueryWithParentId(PropertyInfo[] instances, object item, string? foreignKeyColumn, G? foreignKeyValue)
        {
            bool foreignKeyFound = false;
            string tableName = GetTableName(item);
            string query = $"INSERT INTO {tableName} (";
            var (NamesOfColumns, placeHoldersOfColumn, valuesOfColumns) = FillColumnInfoInQuery(instances, item);
            List<string> columnNames = NamesOfColumns;
            List<string> columnPlaceHolders = placeHoldersOfColumn;
            List<object> values = valuesOfColumns;

            // if insertions include foriegn key 
            if (foreignKeyColumn is not null && foreignKeyValue is not null)
            {
                // then finding the index of that and replacing value
                // of that associated index with given parentid/ foreign key column 
                for (int i = 0; i < columnNames.Count; i++)
                {
                    if (columnNames[i] == foreignKeyColumn)
                    {
                        values[i] = foreignKeyValue;
                        foreignKeyFound = true;
                        break;
                    }
                }
                if (!foreignKeyFound)
                {
                    columnNames.Add(foreignKeyColumn);
                    columnPlaceHolders.Add($"@{foreignKeyColumn}");
                    values.Add(foreignKeyValue);
                }
            }
            // Adding column names in query
            query = AddColumnNamesInQuery(query, columnNames);
            query = $" {query}) VALUES (";
            query = AddColumnPlaceHolderInQuery(query, columnPlaceHolders);
            query = $" {query});";
            ExecuteSQLParameterized(query, columnPlaceHolders, values);
        }
        // ----------------------- Insert Query End --------------------------------
        // ----------------------- Update Query  --------------------------------
        public void PerformUpdate(PropertyInfo[] instances, object item)
        {
            ExecuteUpdateQuery(instances, item);
        }
        public void PerformUpdateOfChildTable(PropertyInfo[] instances, object item, string foreignKeyColumn, G parentId)
        {
            ExecuteUpdateQueryWithParentId(instances, item, foreignKeyColumn, parentId);
        }
        public void ExecuteUpdateQuery(PropertyInfo[] instances, object item)
        {
            //UPDATE table_name
            //SET column1 = value1, column2 = value2, ...
            //WHERE condition;
            G Id = GetValueOfId(item); object record = GetById(Id);
            if (record is not null)
            {
                string tableName = GetTableName(item);
                string query = $"UPDATE {tableName} SET ";
                var (NamesOfColumns, placeHoldersOfColumn, valuesOfColumns) = FillColumnInfoInQuery(instances, item);
                List<string> columnNames = NamesOfColumns;
                List<string> columnPlaceHolders = placeHoldersOfColumn;
                List<object> values = valuesOfColumns;
                // Removing 0th index/Id column (first item) from every elements , as Id shoudln't be updated
                columnNames.Remove(columnNames[0]);
                columnPlaceHolders.Remove(columnPlaceHolders[0]);
                values.Remove(values[0]);
                string lastColumnName = columnNames.Last();
                // Adding column names in query
                for (int i = 0; i < columnNames.Count; i++)
                {
                    if (columnNames[i] != "Id")
                    {
                        if (columnNames[i] == lastColumnName)
                        {
                            query = $"{query} {columnNames[i]}={columnPlaceHolders[i]}";
                        }
                        else
                        {
                            query = $"{query} {columnNames[i]}={columnPlaceHolders[i]},";
                        }
                    }
                }
                columnPlaceHolders.Add("@id");
                values.Add(Id);
                query = $" {query} WHERE Id = @id ;";
                ExecuteSQLParameterized(query, columnPlaceHolders, values);
            }
            else
            {
                Console.WriteLine("Sorry this item doesn't exist");
            }
        }
        public void ExecuteUpdateQueryWithParentId(PropertyInfo[] instances, object item, string? foriegnKeyColumnName, G? foreignKeyValue)
        {
            G Id = GetValueOfId(item);
            object record = GetById(Id);
            if (record is not null)
            {
                string tableName = GetTableName(item);
                string query = $"UPDATE {tableName} SET ";
                var (NamesOfColumns, placeHoldersOfColumn, valuesOfColumns) = FillColumnInfoInQuery(instances, item);
                List<string> columnNames = NamesOfColumns;
                List<string> columnPlaceHolders = placeHoldersOfColumn;
                List<object> values = valuesOfColumns;
                // Removing 0th index/Id column (first item) from every elements , as Id shoudln't be updated
                columnNames.Remove(columnNames[0]);
                columnPlaceHolders.Remove(columnPlaceHolders[0]);
                values.Remove(values[0]);
                string lastColumnName = columnNames.Last();
                // if foreignKey is found then replacing the value with item's parent Id
                for (int i = 0; i < columnNames.Count; i++)
                {
                    if (columnNames[i] == foriegnKeyColumnName)
                    {
                        values[i] = foreignKeyValue;
                    }
                }
                // Adding column names in query
                for (int i = 0; i < columnNames.Count; i++)
                {
                    if (columnNames[i] == lastColumnName)
                    {
                        query = $"{query} {columnNames[i]}={columnPlaceHolders[i]}";
                    }
                    else
                    {
                        query = $"{query} {columnNames[i]}={columnPlaceHolders[i]},";
                    }
                }
                columnPlaceHolders.Add("@id");
                values.Add(Id);
                query = $" {query} WHERE Id = @id ;";
                ExecuteSQLParameterized(query, columnPlaceHolders, values);
            }
            else
            {
                Console.WriteLine("Sorry this item doesn't exist");
            }

        }
        // ----------------------- Update Query End --------------------------------
        // ----------------------- Delete Query -------------------------------------
        public void PerformDeletetion(object item)
        {
            ExecuteDeleteQuery(item);
        }
        public void PerformDeletetionById(G Id)
        {
            ExecuteDeleteByIdQuery(Id);
        }
        public void PerformDeletetionOfChild(object item, string foreignKeyColumn, G parentId)
        {
            ExecuteDeleteQueryWithParentId(item, foreignKeyColumn, parentId);
        }
        public void ExecuteDeleteQuery(object item)
        {
            string tableName = GetTableName(item);
            string query = "";
            var Id = GetValueOfId(item);
            List<string> placeHolders = new List<string>() { "@id" };
            List<object> values = new List<object>() { Id };
            if (Id != null)
            {
                if (IsNumericType(Id.GetType()))
                {
                    query = $"DELETE FROM {tableName} WHERE Id=@id;";
                }
                else
                {
                    query = $"DELETE FROM {tableName} WHERE Id=@id;";
                }
            }
            ExecuteSQLParameterized(query, placeHolders, values);
        }
        public void ExecuteDeleteByIdQuery(G Id)
        {
            string tableName = typeof(T).Name;
            string query = "";
            List<string> placeHolders = new List<string>() { "@id" };
            List<object> values = new List<object>() { Id };
            if (Id != null)
            {
                query = $"DELETE FROM {tableName} WHERE Id=@id;";
            }
            ExecuteSQLParameterized(query, placeHolders, values);
        }
        public void ExecuteDeleteQueryWithParentId(object item, string foreignKeyColumn, G parentId)
        {
            string tableName = GetTableName(item);
            string query = "";
            List<string> placeHolders = new List<string>() { "@foreignKeyColumn" };
            List<object> values = new List<object>() { parentId };
            if (parentId != null)
            {
                query = $"DELETE FROM {tableName} WHERE {foreignKeyColumn}=@foreignKeyColumn;";
            }
            ExecuteSQLParameterized(query, placeHolders, values);
        }
        // ----------------------- Delete Query End -----------------------------------
        // ----------------------- Get By Id Query ---------------------------
        public T PerformGetById(G Id, Type type)
        {
            string tableName = type.Name;
            List<string> placeHolders = new List<string>() { "@id" };
            List<object> values = new List<object>() { Id };
            string query = $"SELECT* FROM {tableName} WHERE ";

            query = $"{query}Id=@id;";
            return ExecuteSQLReaderParameterized(query, placeHolders, values);
        }
        // ----------------------- Get By Id Query End---------------------------
        // ----------------------- GetAll By Id Query ---------------------------
        public IEnumerable<T> PerformGetAll(Type type)
        {
            string tableName = type.Name;
            string query = $"SELECT* FROM {tableName};";
            return ExecuteSQLReader(query);
        }

        // ----------------------- GetAll By Id Query ---------------------------
        // --------------- DB Communication -----------------
        public (List<string>, List<string>, List<object>) FillColumnInfoInQuery(PropertyInfo[] instances, object parentItem)
        {
            List<string> columnNames = new List<string>();
            List<string> placeHolderOfColumn = new List<string>();
            List<object> values = new List<object>();
            foreach (PropertyInfo instance in instances)
            {
                if (!IsIterableType(instance.PropertyType) && !IsComplexType(instance.PropertyType))
                {
                    if (ColumnExistance(instance, parentItem))
                    {
                        //Console.WriteLine($"{instance} found on table {parentItem.GetType().Name}");
                        var (columnName, columnType) = GetInstanceType(instance, parentItem);
                        //Add column Name
                        columnNames.Add(columnName);
                        placeHolderOfColumn.Add($"@{columnName.ToLower()}");
                        //Add column Value
                        var value = instance.GetValue(parentItem);
                        values.Add(value ?? null);
                    }
                    else
                    {
                        //Add column Name
                        columnNames.Add(instance.Name);
                        //Add column Value
                        values.Add(null);
                    }
                }
            }
            return (columnNames, placeHolderOfColumn, values);
        }
        public string AddColumnNamesInQuery(string query, List<string> columnNames)
        {
            var lastColumnName = columnNames.Last();
            foreach (var columnName in columnNames)
            {
                if (columnName == lastColumnName)
                {
                    query = $"{query} {columnName}";
                }
                else
                {
                    query = $"{query} {columnName},";
                }
            }
            return query;
        }
        public string AddColumnPlaceHolderInQuery(string query, List<string> columnPlaceHolders)
        {
            var lastColumnName = columnPlaceHolders.Last();
            foreach (var columnPlaceHolder in columnPlaceHolders)
            {
                if (columnPlaceHolder == lastColumnName)
                {
                    query = $"{query} {columnPlaceHolder}";
                }
                else
                {
                    query = $"{query} {columnPlaceHolder},";
                }
            }
            return query;
        }
        public string AddColumnValuesInQuery(string query, List<object> values)
        {
            var lastValue = values.Last();
            foreach (var value in values)
            {
                if (value is not null)
                {
                    if (value == lastValue)
                    {
                        if (IsNumericType(value.GetType()))
                        {
                            query = $"{query} {value}";
                        }
                        else
                        {
                            query = $"{query} '{value}'";
                        }
                    }
                    else
                    {
                        if (IsNumericType(value.GetType()))
                        {
                            query = $"{query} {value},";
                        }
                        else
                        {
                            query = $"{query} '{value}',";
                        }
                    }
                }
                else
                {
                    if (value == lastValue)
                    {
                        query = $"{query} 'NULL'";
                    }
                    else
                    {
                        query = $"{query} 'NULL',";
                    }
                }

            }
            return query;
        }
        // --------------- DB Communication End -----------------


        // -------------------   DB Utility -------------------------
        public void ExecuteSQL(string query)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.ExecuteScalar();
            }
        }
        public void ExecuteSQLParameterized(string query, List<string> placeHolders, List<object> values)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    for (int i = 0; i < placeHolders.Count; i++)
                    {
                        cmd.Parameters.AddWithValue(placeHolders[i], values[i]);
                    }
                    try
                    {
                        cmd.ExecuteScalar();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An exception happend regarding : {ex.Message}");
                    }
                }
            }
        }
        public IEnumerable<T> ExecuteSQLReader(string query)
        {
            List<T> results = new List<T>();
            Type genericType = typeof(T);
            var props = typeof(T).GetProperties(BindingFlags.Instance|BindingFlags.Public);
            int primitives = 0,iter=0;
            foreach (var prop in props)
            {
                if (!IsComplexType(prop.PropertyType) && !IsIterableType(prop.PropertyType))
                {
                    ++primitives;
                }
            }
            List<Type> typeArguments = new List<Type>();
            int index = 0;
            foreach (var prop in props)
            {
                if (!IsComplexType(prop.PropertyType) && !IsIterableType(prop.PropertyType))
                {
                    typeArguments.Add(prop.PropertyType);
                    ++index;
                }
            }
            Type[] newtypeArguments = typeArguments.ToArray();
            object[] retrievedValues = null;
            using(SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        retrievedValues = new object[reader.FieldCount];
                        // Inserting column names
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            retrievedValues[i] = (reader.GetValue(i));
                            if (i%primitives==0)
                            {
                                ConstructorInfo constructor = genericType.GetConstructor(newtypeArguments);
                                var instance = constructor.Invoke(retrievedValues);
                                results.Add((T)instance);
                            }
                        }
                        //int productId = reader.GetInt32(0);
                        //string productName = reader.GetString(1);
                        //decimal price = reader.GetDecimal(2);

                        // Store or process data as needed
                    }
                }
            }
            return (IEnumerable<T>)results;
        }
        public T ExecuteSQLReaderParameterized(string query, List<string> placeHolders, List<object> values)
        {
            object[] retrievedValues = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    for (int i = 0; i < placeHolders.Count; i++)
                    {
                        cmd.Parameters.AddWithValue(placeHolders[i], values[i]);
                    }
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            retrievedValues = new object[reader.FieldCount];
                            // Inserting column names
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                 retrievedValues[i] = (reader.GetValue(i));
                            }
                            //int productId = reader.GetInt32(0);
                            //string productName = reader.GetString(1);
                            //decimal price = reader.GetDecimal(2);
                        }
                    }
                }
            }
            Type genericType = typeof(T);
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public );
            List<Type>typeArguments = new List<Type>(); 
            int index = 0;
            foreach (var prop in props)
            {
                if(!IsComplexType(prop.PropertyType) && !IsIterableType(prop.PropertyType))
                {
                    typeArguments.Add(prop.PropertyType);
                    ++index;
                }
            }
            Type[] newtypeArguments = typeArguments.ToArray();
            ConstructorInfo constructor = genericType.GetConstructor(newtypeArguments);
            var instance = constructor.Invoke(retrievedValues); 
            
            return (T)instance;
        }
        // -------------------   DB Utility -------------------------
        // -------------------   Existance Checking  ---------------------
        public G GetValueOfId(object item)
        {
            var type = item.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public );
            G Id = default;
            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;
                if (property.Name == "Id")
                {
                    Id = (G)property.GetValue(item);
                    break;
                }
            }
            return Id;
        }
        public string GetForiegnKey(object parent)
        {
            return $"{GetTableName(parent)}Id";
        }
        public bool ColumnExistance(object instance, object parentItem)
        {
            var tableColumn = GetTableColumn(parentItem);
            var typeColumn = GetInstanceType((PropertyInfo)instance, parentItem);
            string instanceName = typeColumn.Item1;
            Type instanceType = typeColumn.Item2;
            return HasColumn(instanceName, instanceType, tableColumn);
        }
        public bool HasColumn(string instanceName, Type instanceType, DataTable tableColumn)
        {
            bool exist = false;
            foreach (DataRow row in tableColumn.Rows)
            {
                string columnName = row["COLUMN_NAME"].ToString();
                string dataType = row["DATA_TYPE"].ToString();

                if (instanceName == columnName) //name comparison 
                {
                    Type tableType = GetClrTypeFromSqlType(dataType);
                    if (instanceType == tableType)
                    {
                        exist = true;
                        break;
                    }
                }
            }
            return exist;
        }
        public bool ForeignKeyExistance(string foreignKey, object Item)
        {
            var tableColumn = GetTableColumn(Item);
            return HasColumn(foreignKey, typeof(G), tableColumn);
        }
        public bool TableExistance(Type type)
        {
            if (type == typeof(PropertyInfo))
            {
                type = type.GetType();
            }
            string tableName = type.Name;
            return TableExist(tableName);
        }
        public bool TableExist(string tableName)
        {
            int count = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
                SqlCommand cmd = new SqlCommand(query, connection);
                count = (int)cmd.ExecuteScalar();
            }
            return count > 0;
        }
        public bool SubTableExistance(object child, Type childType, object parent, Type parentType)
        {
            bool subTableExistance = false, tableExistance, foreignKey;
            string tableName = "", foreignKeyColumnName = "", parentTableName = "";
            if (childType.IsGenericType)
            {
                tableName = childType.GetGenericArguments()[0].Name;
            }
            if (IsComplexType(childType))
            {
                tableName = childType.Name;
            }
            if (parentType.IsGenericType)
            {
                parentTableName = parentType.GetGenericArguments()[0].Name;
            }
            if (IsComplexType(parentType))
            {
                parentTableName = parentType.Name;
            }
            foreignKeyColumnName = $"{parentTableName}Id";
            return ForeignKeyExistance(foreignKeyColumnName, child) && TableExist(tableName);
        }
        // -------------------   Existance Checking  ---------------------

        // -------------------   Type Checking  ---------------------
        public bool IsComplexType(Type type)
        {
            return !type.IsPrimitive && !type.IsGenericType
                && (type != typeof(string))
                && (type != typeof(Guid))
                && (type != typeof(DateTime))
                && (type != typeof(decimal));
        }
        public bool IsIterableType(Type type)
        {
            return type.IsGenericType || type.IsArray;
        }
        public bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(uint);
        }
        public bool HasAllPrimitiveType(object item)
        {
            bool hasAllPrimitiveType = true;
            var type = item.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public );
            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;
                if (IsIterableType(propertyType) || IsComplexType(property.PropertyType))
                {
                    hasAllPrimitiveType = false;
                    break;
                }
            }
            return hasAllPrimitiveType;
        }
        // -------------------   Type Checking  ---------------------

        // -------------------   DB Utilities  ---------------------
        public DataTable GetTableColumn(object item)
        {
            DataTable table;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string tableName = GetTableName(item);
                table = connection.GetSchema("Columns", new string[] { null, null, tableName });
            }
            return table;
        }
        public string GetTableName(object item)
        {
            return item.GetType().Name;
        }
        public (string, Type) GetInstanceType(PropertyInfo property, object parentItem)
        {
            (string instanceName, Type instanceType) typeColumn = (null, null);
            var value = property.GetValue(parentItem);
            if (value is not null)
            {
                typeColumn.instanceType = value.GetType();
                typeColumn.instanceName = property.Name;
            }
            return typeColumn;
        }
        public static Type GetClrTypeFromSqlType(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "uniqueidentifier":
                    return typeof(Guid);
                case "bigint":
                    return typeof(double);
                case "int":
                    return typeof(int);
                case "float":
                    return typeof(double);
                case "nvarchar":
                case "varchar":
                    return typeof(string);
                case "datetime":
                    return typeof(DateTime);
                // Add more mappings as needed
                default:
                    return typeof(object);
            }
        }
        // -------------------   DB Utilities  ---------------------
    }
}

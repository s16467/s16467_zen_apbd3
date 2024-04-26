using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using s16467_zen_apbd3.Model;

namespace s16467_zen_apbd3.Controllers
{
    [Route("api/animals")]
    [ApiController]
    public class AnimalController : ControllerBase
    {
        static string connectToBase = "Data Source=db-mssql16.pjwstk.edu.pl;Initial Catalog=s16467;Integrated Security=True";
        
        [HttpGet]
        public IActionResult GetAnimal(String orderBy = "name")
        {
            string queryString = "SELECT * FROM Animal ORDER BY " + orderBy + " ASC";
            string print = " ";

            if (orderBy.ToLower() != "name" && orderBy.ToLower() != "description" && orderBy.ToLower() != "category" && orderBy.ToLower() != "area")
            {
                return BadRequest("sth went wrong");
            }

            using (SqlConnection connection = new SqlConnection(connectToBase))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    print += ReadSingleRecord((IDataRecord)reader) + "\n";
                }


                reader.Close();
            }
            return Ok(print);
        }

        [HttpPost]
        public IActionResult AddAnimal([FromBody] Animal content)
        {
            if (IsAnyNullOrEmpty(content))
            {
                return BadRequest("Missing >=1 parameter in JSON");
            }

            string queryString = "INSERT INTO Animal (name, description, category, area)VALUES ('" 
                + content.Name + "','" + content.Description + "','" + content.Category + "','" + content.Area + "')";


            using (SqlConnection connection = new SqlConnection(connectToBase))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (SqlException e) { return BadRequest("Execution querty error:" + e.Message); }

                catch (Exception e) { return BadRequest("Connection error:" + e.Message); }

                finally { connection.Close(); }


            }

            return Ok();
        }

        [HttpPut("{idAnimal}")]
        public IActionResult EditAnimal([FromBody] Animal content, [FromRoute] int idAnimal)
        {
            if (IsAnyNullOrEmpty(content)) { return BadRequest("Missing >=1 parameter in JSON"); }

            string queryString = "UPDATE Animal SET name = '" + content.Name + "',description = '" + content.Description + "',category = '" 
                                + content.Category + "',area = '" + content.Area + "' WHERE idAnimal = " + idAnimal;

            using (SqlConnection connection = new SqlConnection(connectToBase))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (SqlException e) { return BadRequest("Execution querty error:" + e.Message); }

                catch (Exception e) { return BadRequest("Connection error:" + e.Message); }

                finally { connection.Close(); }
            }

            return Ok();
        }

        [HttpDelete("{idAnimal}")]
        public IActionResult DeleteAnimal([FromRoute] int idAnimal)
        {

            string queryString = "DELETE FROM Animal WHERE idAnimal = " + idAnimal;
            int row = 0;
            using (SqlConnection connection = new SqlConnection(connectToBase))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                try
                {
                    connection.Open();
                    row = command.ExecuteNonQuery();
                }
                catch (SqlException e) { return BadRequest("Execution querty error:" + e.Message); }

                catch (Exception e) { return BadRequest("Connection error:" + e.Message); }

                finally { connection.Close(); }
            }

            if (!Convert.ToBoolean(row))
            {
                return BadRequest("idAnimal" + Convert.ToString(idAnimal) + " not found");
            }

            return Ok();
        }

        bool IsAnyNullOrEmpty(object myAnimal)
        {
            foreach (PropertyInfo propInfo in myAnimal.GetType().GetProperties())
            {
                if (propInfo.PropertyType == typeof(string))
                {
                    string value = (string)propInfo.GetValue(myAnimal);

                    if (string.IsNullOrEmpty(value)) { return true; }
                }
            }
            return false;
        }
        private static string ReadSingleRecord(IDataRecord record)
        {
            return String.Format("{0}, {1}, {2}, {3}, {4}", record[0], record[1], record[2], record[3], record[4]);
        }
    }
}

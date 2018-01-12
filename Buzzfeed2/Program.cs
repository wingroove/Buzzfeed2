using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buzzfeed2
{
    class Program
    {


        static void Main(string[] args)
        {
            bool keepGoing = true;

            int quizPick;
            int answerInput;
            int currentUser = 0;
            int counter = 0;
            int resultTotal;
            int answerValue = 0;

            string finalResult;

            //below connection accesses the database on this computer
            SqlConnection connection = new SqlConnection(@"Data Source=(LocalDb)\MSSQLLocalDB;AttachDbFilename=c:\users...Buzzfeed2\Database1.mdf;Integrated Security=True");
            connection.Open();

            while (keepGoing)
            {
                //Display the list of quizzes
                resultTotal = 0;

                Console.WriteLine("Here are the available quizzes:");

                SqlCommand quizList = new SqlCommand($"Select * FROM Quiz", connection);

                SqlDataReader reader;
                reader = quizList.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Id"]} ) {reader["Title"]}");
                    }
                }

                reader.Close();

                //Ask which quiz the user would like to take

                Console.WriteLine("Please type the number of the quiz that you would like to take.");
                quizPick = Convert.ToInt32(Console.ReadLine());

                SqlCommand newUser = new SqlCommand($"INSERT INTO Users (QuizId) VALUES ('{quizPick}'); SELECT @@Identity AS ID", connection);

                reader = newUser.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    currentUser = Convert.ToInt32(reader["ID"]);
                }

                reader.Close();
                //display the first question and answers

                List<Question> questions = new List<Question>();

                SqlCommand command = new SqlCommand($"SELECT * FROM Questions WHERE QuizId = {quizPick} ORDER BY Questions.id", connection);

                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Question questionInstance = new Question();
                        questionInstance.questionId = Convert.ToInt32(reader["Id"]);
                        questionInstance.questionText = reader["Question"].ToString();

                        questions.Add(questionInstance);
                    }
                }
                reader.Close();

                foreach (Question q in questions)
                {
                    command = new SqlCommand($"SELECT * FROM Answers WHERE QuestionId={q.questionId}", connection);
                    reader = command.ExecuteReader();
                    q.answers = new List<Answer>();
                    if (reader.HasRows)
                    {
                        counter = 1;

                        while (reader.Read())
                        {
                            Answer answerInstance = new Answer();
                            answerInstance.answerId = Convert.ToInt32(reader["Id"]);
                            answerInstance.answerText = reader["Answer"].ToString();
                            answerInstance.answerValue = Convert.ToInt32(reader["Value"]);
                            answerInstance.choose = counter;
                            counter++;
                            q.answers.Add(answerInstance);
                        }
                    }
                    reader.Close();

                    Console.WriteLine($"{q.questionText}");

                    foreach (Answer a in q.answers)
                    {
                        Console.WriteLine($"   {a.choose}) {a.answerText}");
                    }
                    Console.WriteLine("Please choose an answer. Type the corresponding number.");

                    answerInput = Convert.ToInt32(Console.ReadLine());
                    foreach (Answer a in q.answers)
                    {
                        if (a.choose==answerInput)
                        {
                            answerValue = a.answerId;
                        }
                    }

                    command = new SqlCommand($"INSERT INTO Responses (AnswerId, UserId) VALUES ('{answerValue}', '{currentUser}')", connection);

                    command.ExecuteNonQuery();
                }
                //respeat as needed

                //calculate results

                //access the response table, join with the answers table
                SqlCommand getResponseValues = new SqlCommand($"SELECT * FROM Answers JOIN Responses ON Answers.Id = Responses.AnswerId WHERE Responses.UserId ={currentUser}", connection);
                //make a resultTotal and add the answer.value into it
                reader = getResponseValues.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        resultTotal = resultTotal + Convert.ToInt32(reader["Value"]);
                    }
                }
                reader.Close();

                //access the result table
                SqlCommand compareResults = new SqlCommand($"SELECT * FROM Results WHERE QuizID ='{quizPick}'", connection);
                //compare resultTotal to the result.value
                reader = compareResults.ExecuteReader();

                //display result

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(resultTotal);
                        if (resultTotal <= Convert.ToInt32(reader["Score"]))
                        {
                            finalResult = reader["Result"].ToString();
                            Console.WriteLine($"Your final result is: {finalResult}");
                            break;
                        }
                    }
                }

                reader.Close();
                Console.WriteLine("Would you like to take another quiz? y/n");

                if (Console.ReadLine() == "n")
                {
                    keepGoing = false;
                    connection.Close();
                }
                Console.WriteLine();
            }
        }
    }

    class Question
    {
        public int questionId;
        public string questionText;
        public List<Answer> answers;
    }

    class Answer
    {
        public int answerId;
        public string answerText;
        public int answerValue;
        public int choose;
    }
}

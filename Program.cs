using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;

namespace RedisSubscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            var connString = "localhost";
            var redis = ConnectionMultiplexer.Connect(connString);
            var sub = redis.GetSubscriber();

            Console.WriteLine($"iniciando conexão");

            var db = redis.GetDatabase();

            try
            {
                sub.Subscribe("perguntas").OnMessage(
                c =>
                {
                    string message = ResolveMessage(c.Message.ToString(), db);
                    Console.WriteLine($"resposta: {message}");
                    Console.WriteLine($"mensagem: {c.Message}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"erro: {ex.Message}");
            }



            Console.ReadLine();
            //P1: Quanto é 1 + 1
            //P1: grupo2, 2
        }

        static string ResolveMessage(string value, IDatabase db)
        {

            string[] pergunta = value.Split(":");
            string hashSetKey = pergunta[0];
            RedisValue resposta = new RedisValue();

            try
            {
                Array per = value.Split(" ");
                var calculo = CSharpScript.EvaluateAsync(per.GetValue(3).ToString().Replace("?", ""));
                resposta = calculo
                    .GetAwaiter()
                    .GetResult()
                    .ToString();
            }
            finally
            {
                if (resposta.HasValue)
                    db.HashSet(hashSetKey, new RedisValue("Grupo 2"), resposta);
                else
                    db.HashSet(hashSetKey, new RedisValue("Grupo 2"), new RedisValue("Ainda não consigo responder essa pergunta."));
            }

            return resposta.ToString();
        }
    }
}

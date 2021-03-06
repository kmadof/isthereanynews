﻿using System;
using System.Collections;
using System.IO;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Itan.Database
{
    internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EntityFrameworkContext>
    {
        public EntityFrameworkContext CreateDbContext(string[] args)
        {

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                // .AddAzureKeyVault("https://itan-key-vault.vault.azure.net", kvc, new DefaultKeyVaultSecretManager())
                .Build();

            var connectionString = Environment.GetEnvironmentVariable("var-6");
            var x1 = Environment.GetEnvironmentVariable("SqlAdminConnectionString", EnvironmentVariableTarget.Machine);
            var x2 = Environment.GetEnvironmentVariable("SqlAdminConnectionString", EnvironmentVariableTarget.Process);
            var x3 = Environment.GetEnvironmentVariable("SqlAdminConnectionString", EnvironmentVariableTarget.User);

            var e = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry dictionaryEntry in e)
            {
                Console.WriteLine($"K:{dictionaryEntry.Key} - V:{dictionaryEntry.Value}");
            }

            Console.WriteLine("CS:"+connectionString);
            Console.WriteLine("x1:"+x1);
            Console.WriteLine("x2:"+x2);
            Console.WriteLine("x3:"+x3);

            var builder = new DbContextOptionsBuilder<EntityFrameworkContext>();
            // var connectionString =
            //     kvc.GetSecretAsync("https://itan-key-vault.vault.azure.net/", "SqlAdminConnectionString")
            //        .GetAwaiter()
            //        .GetResult()
            //        .Value;
            builder.UseSqlServer(connectionString);
            return new EntityFrameworkContext(builder.Options);
        }
    }
}
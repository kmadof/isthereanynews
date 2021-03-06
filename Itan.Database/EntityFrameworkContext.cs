﻿using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Itan.Database
{
    internal class EntityFrameworkContext : DbContext
    {
        public EntityFrameworkContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Channel> Channels { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<ChannelDownload> ChannelDownloads { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<ChannelsPersons> ChannelsPersons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureNews(modelBuilder);
            ConfigureChannel(modelBuilder);
            ConfigurePerson(modelBuilder);
            ConfigureChannelsPersons(modelBuilder);
            ConfigureChannelDownload(modelBuilder);
            ConfigureChannelSubmitter(modelBuilder);
            ConfigureChannelNewsRead(modelBuilder);
        }

        private void ConfigureChannelNewsRead(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChannelNewsRead>()
                .HasIndex(x => new {x.ChannelId, x.NewsId, x.PersonId})
                .IsUnique();
        }

        private void ConfigureChannelSubmitter(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChannelSubmitter>()
                .HasIndex(x => x.ChannelId)
                .IsUnique();
        }

        private void ConfigureChannelDownload(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChannelDownload>()
                .HasIndex(x => new {x.ChannelId, x.SHA256})
                .IsUnique(true);
        }

        private void ConfigureChannelsPersons(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChannelsPersons>()
                .HasOne<Channel>(x => x.Channel)
                .WithMany(x => x.PersonSubscribers)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChannelsPersons>()
                .HasOne<Person>(x => x.Reader)
                .WithMany(x => x.SubscribedChannels)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigurePerson(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .HasMany<ChannelSubmitter>(x => x.SubmittedChannels)
                .WithOne(x => x.Submitter)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Person>()
                .HasMany<ChannelNewsRead>(x => x.ChannelNewsRead)
                .WithOne(x => x.Person)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureChannel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Channel>()
                .HasOne<ChannelSubmitter>(x => x.Submitter)
                .WithOne(x => x.Channel)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Channel>()
                .HasMany<ChannelNewsRead>(x => x.ChannelNewsRead)
                .WithOne(x => x.Channel)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureNews(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<News>()
                .HasIndex(x => new {x.ChannelId, x.SHA256})
                .IsUnique(true);

            modelBuilder.Entity<News>()
                .HasOne<Channel>(x => x.Channel)
                .WithMany(x => x.News)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<News>()
                .HasMany<ChannelNewsRead>(x => x.ChannelNewsRead)
                .WithOne(x => x.News)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
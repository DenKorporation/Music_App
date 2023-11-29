﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MusicCatalogService.Data;

#nullable disable

namespace MusicCatalogService.Migrations
{
    [DbContext(typeof(MusicCatalogDbContext))]
    [Migration("20231129222627_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("MusicCatalogService.Models.Artist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("MusicCatalogService.Models.Genre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("MusicCatalogService.Models.Playlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("MusicCatalogService.Models.Track", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("ArtistId")
                        .HasColumnType("int");

                    b.Property<float>("Duration")
                        .HasColumnType("float");

                    b.Property<int>("GenreId")
                        .HasColumnType("int");

                    b.Property<DateOnly>("ReleaseDate")
                        .HasColumnType("date");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("ArtistId");

                    b.HasIndex("GenreId");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("MusicCatalogService.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("ExternalId")
                        .HasColumnType("int");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PlaylistTrack", b =>
                {
                    b.Property<int>("PlaylistsId")
                        .HasColumnType("int");

                    b.Property<int>("TracksId")
                        .HasColumnType("int");

                    b.HasKey("PlaylistsId", "TracksId");

                    b.HasIndex("TracksId");

                    b.ToTable("PlaylistTrack");
                });

            modelBuilder.Entity("MusicCatalogService.Models.Playlist", b =>
                {
                    b.HasOne("MusicCatalogService.Models.User", "User")
                        .WithMany("Playlists")
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MusicCatalogService.Models.Track", b =>
                {
                    b.HasOne("MusicCatalogService.Models.Artist", "Artist")
                        .WithMany("Tracks")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MusicCatalogService.Models.Genre", "Genre")
                        .WithMany("Tracks")
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");

                    b.Navigation("Genre");
                });

            modelBuilder.Entity("PlaylistTrack", b =>
                {
                    b.HasOne("MusicCatalogService.Models.Playlist", null)
                        .WithMany()
                        .HasForeignKey("PlaylistsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MusicCatalogService.Models.Track", null)
                        .WithMany()
                        .HasForeignKey("TracksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MusicCatalogService.Models.Artist", b =>
                {
                    b.Navigation("Tracks");
                });

            modelBuilder.Entity("MusicCatalogService.Models.Genre", b =>
                {
                    b.Navigation("Tracks");
                });

            modelBuilder.Entity("MusicCatalogService.Models.User", b =>
                {
                    b.Navigation("Playlists");
                });
#pragma warning restore 612, 618
        }
    }
}
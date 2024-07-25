using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    public partial class AddDefaultValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Admin",
                columns: new[] { "admin_ID", "name", "email", "password", "isSuper" },
                values: new object[,] 
                {
                    { "D56F556E-50A4-4240-A0FF-9A6898B3A03B", "Joel", "joel@example.com", "securepassword", true },
                    { "cb2d0c01-ef51-4deb-96dd-f57c25497fe3", "Jane Doe", "123@", "Kw7+jFXwfGw/o6Mi2vJEXw==", true },
                }
            );

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "role_ID", "roleName", "description" },
                values: new object[,]
                {
                    { "Admin", "Admin", "Administrator with full access to the system" },
                    { "Support Agent", "Support Agent", "Support agent with access to help desk functionalities" },
                    { "Employee", "Employee", "Employee with basic access to the system" }
                }
            );

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "user_ID", "name", "email", "password", "role_ID", "createdBy", "createdTime", "updatedTime", "updatedBy" },
                values: new object[,]
                {
                    { "cb2d0c01-ef51-4deb-96dd-f57c25497fe3", "Jane Doe", "123@", "Kw7+jFXwfGw/o6Mi2vJEXw==", "Admin",
                        "D56F556E-50A4-4240-A0FF-9A6898B3A03B", DateTime.Now, DateTime.Now, "D56F556E-50A4-4240-A0FF-9A6898B3A03B" },
                    { "5c8185f8-a1ce-4175-8179-f9a055c8d3c4", "Agent A", "agenta@ticketita.com", "WDCHwzavyvUzFb/JBYNiCg==", "Support Agent",
                        "D56F556E-50A4-4240-A0FF-9A6898B3A03B", DateTime.Now, DateTime.Now, "D56F556E-50A4-4240-A0FF-9A6898B3A03B" },
                    { "71e6123f-641d-42fe-937f-07a5cd27977f", "Agent B", "agentb@ticketita.com", "WDCHwzavyvUzFb/JBYNiCg==", "Support Agent",
                        "D56F556E-50A4-4240-A0FF-9A6898B3A03B", DateTime.Now, DateTime.Now, "D56F556E-50A4-4240-A0FF-9A6898B3A03B" },
                    { "9d6b7566-8c39-4027-a1dd-e96411cdb907", "John Doe", "employeea@ticketita.com", "WDCHwzavyvUzFb/JBYNiCg==", "Employee",
                        "D56F556E-50A4-4240-A0FF-9A6898B3A03B", DateTime.Now, DateTime.Now, "D56F556E-50A4-4240-A0FF-9A6898B3A03B" },
                    { "ddcd70c8-9939-4159-9f09-5377c6478167", "James Gabriel", "employeeb@ticketita.com", "WDCHwzavyvUzFb/JBYNiCg==", "Employee",
                        "D56F556E-50A4-4240-A0FF-9A6898B3A03B", DateTime.Now, DateTime.Now, "D56F556E-50A4-4240-A0FF-9A6898B3A03B" }
                }
            );

            migrationBuilder.InsertData(
                table: "PriorityType",
                columns: new[] { "priorityType_ID", "priorityName", "description" },
                values: new object[,]
                {
                    { "P1", "Critical", "Ticket with the highest priority." },
                    { "P2", "High", "Ticket with high priority." },
                    { "P3", "Medium", "Ticket with medium priority." },
                    { "P4", "Low", "Ticket with low priority." },
                }
            );

            migrationBuilder.InsertData(
                table: "StatusType",
                columns: new[] { "statusType_ID", "statusName", "description" },
                values: new object[,]
                {
                    { "S1", "Open", "Ticket is open." },
                    { "S2", "In Progress", "Ticket is in progress." },
                    { "S3", "Resolved", "Ticket is marked as resolved." },
                    { "S4", "Closed", "Ticket is closed with or without resolution." },
                }
            );

            migrationBuilder.InsertData(
                table: "CategoryType",
                columns: new[] { "categoryType_ID", "categoryName", "description" },
                values: new object[,]
                {
                    { "C1", "Software", "Software related issues." },
                    { "C2", "Hardware", "Hardware related issues." },
                    { "C3", "Network", "Network related issues." },
                    { "C4", "Account", "Account related issues." },
                    { "C5", "Other", "Other issues not categorized." }
                }
            );

            migrationBuilder.InsertData(
                table: "Team",
                columns: new[] { "team_ID", "name", "description", "specialization_ID" },
                values: new object[,]
                {
                    { $"{Guid.NewGuid().ToString()}", "Software Saviors", "The team specializes in software related issues.", "C1" },
                    { $"{Guid.NewGuid().ToString()}", "Hardware Heroes", "The team specializes in hardware related issues.", "C2" },
                    { $"{Guid.NewGuid().ToString()}", "Network Ninjas", "The team specializes in network related issues.", "C3" },
                    { $"{Guid.NewGuid().ToString()}", "Access Aces", "The team specializes in account related issues.", "C4" },
                }
            );

            migrationBuilder.InsertData(
                table: "ArticleCategory",
                columns: new[] { "category_ID", "categoryName", "description" },
                values: new object[,]
                {
                    { "1", "Getting Started", "Articles on how to get started." },
                    { "2", "Troubleshooting", "Articles on troubleshooting." },
                    { "3", "Product Features", "Articles on features of a product." },
                    { "4", "How-to Guides", "Articles on processes." },
                    { "5", "FAQs", "Articles on frequently asked questions." },
                    { "6", "Best Practices", "Articles on the best practices." },
                    { "7", "Release Notes", "Articles on release notes." },
                    { "8", "Policies and Procedures", "Articles on policies and procedures." },
                    { "9", "Technical Documentation", "Articles on technical documentations." },
                    { "10", "Account Management", "Articles on the management of accounts." }
                }
            );

            migrationBuilder.InsertData(
                table: "NotificationType",
                columns: new[] { "notificationType_ID", "title", "description" },
                values: new object[,]
                {
                    { "1", "Ticket Created", "Notification to user upon ticket creation" },
                    { "2", "Ticket Priority Update", "Notification to user and support agent upon ticket priority update" },
                    { "3", "Ticket Status Update", "Notification to user and support agent upon ticket status update" },
                    { "4", "Ticket Details Update", "Notification to user and support agent upon ticket details update" },
                    { "5", "Ticket Assignment Update", "Notification to employee and support agent upon ticket assignment changes" },
                    { "6", "Ticket New Comment", "Notification when there is a new ticket comment" },
                    { "7", "Ticket New Feedback", "Notification when there is a new ticket feedback" },
                    { "8", "Ticket Reminder", "Notification to support agents to remind on unresolved tickets" },
                    { "9", "Team Assignment Update", "Notification to support agents when there are updates on their team assignment" },
                    { "10", "Forgot Password", "Notification to admin upon user request of change password" },
                }
            );
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;

namespace OwlCoinV2.Backend.Shared.Data
{
    public class SQL
    {
        private OleDbConnection Conn;
        private OleDbCommand Command;
        private string DBase = "";
        public SQL(string DataBase)
        {
            DBase = DataBase;
            RestartConn();
        }

        private void RestartConn()
        {
            if (Conn != null) { if (Conn.State == System.Data.ConnectionState.Open) { Conn.Close(); } }
            Conn = new OleDbConnection("Provider = Microsoft.ACE.OLEDB.12.0; Data Source = " + DBase + ".accdb");
            Conn.Open();
        }

        public void Insert(string Table, int PriKey, string[] Data)
        {
            string Values = PriKey.ToString();
            foreach (string D in Data) { Values = Values + "," + D; }
            Command = new OleDbCommand("INSERT INTO " + Table + " Values (" + Values + ");", Conn);
            Execute();
        }
        public void Insert(string Table, string[] Data)
        {
            string Values = "DEFAULT";
            foreach (string D in Data) { Values = Values + "," + D; }
            Command = new OleDbCommand("INSERT INTO " + Table + " Values (" + Values + ");", Conn);
            Execute();
        }
        public void Insert(string Table,string[] Columns,string[] Data)
        {
            string ColumnString = Columns[0]; Columns[0] = "";
            if (Columns.Length > 1) { foreach (string S in Columns) { ColumnString = ColumnString + "," + S; } }
            string DataString = Data[0]; Data[0] = "";
            if (Data.Length > 1) { foreach (string S in Data) { DataString = DataString + "," + S; } }
            Command = new OleDbCommand("INSERT INTO " + Table + " (" + ColumnString + ") VALUES (" + DataString + ");",Conn);
            Execute();
        }
        public void Delete(string Table)
        {
            Command = new OleDbCommand("DROP TABLE " + Table + ";", Conn);
            Execute();
        }
        public void Delete(string Table, string DeterminantStatment)
        {
            Command = new OleDbCommand("DELETE FROM " + Table + " WHERE " + DeterminantStatment, Conn);
            Execute();
        }
        public void Update(string Table, string DeterminantStatment, string SetStatment)
        {
            Command = new OleDbCommand("UPDATE " + Table + " SET " + SetStatment + " WHERE " + DeterminantStatment, Conn);
            Execute();
        }
        public string[] Select(string Table, string SelectStatment)
        {
            Command = new OleDbCommand("SELECT " + SelectStatment + " FROM " + Table, Conn);
            return ExecuteReader(Command).ToArray<string>();
        }
        public string[] Select(string Table, string SelectStatment, string DeterminantStatment)
        {
            Command = new OleDbCommand("SELECT " + SelectStatment + " FROM " + Table + " WHERE " + DeterminantStatment, Conn);
            return ExecuteReader(Command).ToArray<string>();
        }

        List<String> ExecuteReader(OleDbCommand Command)
        {
            OleDbDataReader Results = Command.ExecuteReader();
            List<String> LResults = new List<string> { };
            while (Results.Read()) { LResults.Add(Results[0].ToString()); }
            Results.Close();
            return LResults;
        }

        void Execute()
        {
            try { Command.ExecuteNonQuery(); /*RestartConn();*/ } catch (Exception E) { Console.WriteLine(E.Message); }
        }
    }
}
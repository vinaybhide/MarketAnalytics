using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class editpfname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_PORTFOLIO_MASTER_USER_MASTER_userMasterUSER_MASTER_ID",
            //    table: "PORTFOLIO_MASTER");

            //migrationBuilder.DropIndex(
            //    name: "IX_PORTFOLIO_MASTER_userMasterUSER_MASTER_ID",
            //    table: "PORTFOLIO_MASTER");

            //migrationBuilder.DropColumn(
            //    name: "USER_MASTER_ID",
            //    table: "PORTFOLIO_MASTER");

            //migrationBuilder.DropColumn(
            //    name: "userMasterUSER_MASTER_ID",
            //    table: "PORTFOLIO_MASTER");

            //migrationBuilder.RenameColumn(
            //    name: "TXN_DATE",
            //    table: "PORTFOLIOTXN",
            //    newName: "TXN_SELL_DATE");

            //migrationBuilder.RenameColumn(
            //    name: "QUANTITY",
            //    table: "PORTFOLIOTXN",
            //    newName: "SOLD_AFTER");

            //migrationBuilder.RenameColumn(
            //    name: "COST_PER_SHARE",
            //    table: "PORTFOLIOTXN",
            //    newName: "TOTAL_SELL_AMT");

            //migrationBuilder.AddColumn<double>(
            //    name: "FastK",
            //    table: "StockMaster",
            //    type: "REAL",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<double>(
            //    name: "RSI_CLOSE",
            //    table: "StockMaster",
            //    type: "REAL",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<bool>(
            //    name: "RSI_OVERBOUGHT",
            //    table: "StockMaster",
            //    type: "INTEGER",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<bool>(
            //    name: "RSI_OVERSOLD",
            //    table: "StockMaster",
            //    type: "INTEGER",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "RSI_TREND_LastUpDt",
            //    table: "StockMaster",
            //    type: "TEXT",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "STOCH_BUYSELL_LastUpDt",
            //    table: "StockMaster",
            //    type: "TEXT",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            //migrationBuilder.AddColumn<double>(
            //    name: "STOCH_BUY_PRICE",
            //    table: "StockMaster",
            //    type: "REAL",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<bool>(
            //    name: "STOCH_BUY_SIGNAL",
            //    table: "StockMaster",
            //    type: "INTEGER",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<double>(
            //    name: "STOCH_SELL_PRICE",
            //    table: "StockMaster",
            //    type: "REAL",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<bool>(
            //    name: "STOCH_SELL_SIGNAL",
            //    table: "StockMaster",
            //    type: "INTEGER",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<double>(
            //    name: "SlowD",
            //    table: "StockMaster",
            //    type: "REAL",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<double>(
            //    name: "CAGR",
            //    table: "PORTFOLIOTXN",
            //    type: "REAL",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<double>(
            //    name: "COST_PER_UNIT",
            //    table: "PORTFOLIOTXN",
            //    type: "REAL",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<double>(
            //    name: "PURCHASE_QUANTITY",
            //    table: "PORTFOLIOTXN",
            //    type: "REAL",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<double>(
            //    name: "SELL_AMT_PER_UNIT",
            //    table: "PORTFOLIOTXN",
            //    type: "REAL",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<double>(
            //    name: "SELL_GAIN_AMT",
            //    table: "PORTFOLIOTXN",
            //    type: "REAL",
            //    nullable: true);

            //migrationBuilder.AddColumn<double>(
            //    name: "SELL_GAIN_PCT",
            //    table: "PORTFOLIOTXN",
            //    type: "REAL",
            //    nullable: true);

            //migrationBuilder.AddColumn<double>(
            //    name: "SELL_QUANTITY",
            //    table: "PORTFOLIOTXN",
            //    type: "REAL",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "TXN_BUY_DATE",
            //    table: "PORTFOLIOTXN",
            //    type: "TEXT",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            //migrationBuilder.AlterColumn<string>(
            //    name: "PORTFOLIO_NAME",
            //    table: "PORTFOLIO_MASTER",
            //    type: "TEXT",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "TEXT");

            //migrationBuilder.AddColumn<string>(
            //    name: "Id",
            //    table: "PORTFOLIO_MASTER",
            //    type: "TEXT",
            //    nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FastK",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "RSI_CLOSE",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "RSI_OVERBOUGHT",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "RSI_OVERSOLD",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "RSI_TREND_LastUpDt",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "STOCH_BUYSELL_LastUpDt",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "STOCH_BUY_PRICE",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "STOCH_BUY_SIGNAL",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "STOCH_SELL_PRICE",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "STOCH_SELL_SIGNAL",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "SlowD",
                table: "StockMaster");

            migrationBuilder.DropColumn(
                name: "CAGR",
                table: "PORTFOLIOTXN");

            migrationBuilder.DropColumn(
                name: "COST_PER_UNIT",
                table: "PORTFOLIOTXN");

            migrationBuilder.DropColumn(
                name: "PURCHASE_QUANTITY",
                table: "PORTFOLIOTXN");

            migrationBuilder.DropColumn(
                name: "SELL_AMT_PER_UNIT",
                table: "PORTFOLIOTXN");

            migrationBuilder.DropColumn(
                name: "SELL_GAIN_AMT",
                table: "PORTFOLIOTXN");

            migrationBuilder.DropColumn(
                name: "SELL_GAIN_PCT",
                table: "PORTFOLIOTXN");

            migrationBuilder.DropColumn(
                name: "SELL_QUANTITY",
                table: "PORTFOLIOTXN");

            migrationBuilder.DropColumn(
                name: "TXN_BUY_DATE",
                table: "PORTFOLIOTXN");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PORTFOLIO_MASTER");

            //migrationBuilder.RenameColumn(
            //    name: "TXN_SELL_DATE",
            //    table: "PORTFOLIOTXN",
            //    newName: "TXN_DATE");

            //migrationBuilder.RenameColumn(
            //    name: "TOTAL_SELL_AMT",
            //    table: "PORTFOLIOTXN",
            //    newName: "COST_PER_SHARE");

            //migrationBuilder.RenameColumn(
            //    name: "SOLD_AFTER",
            //    table: "PORTFOLIOTXN",
            //    newName: "QUANTITY");

            migrationBuilder.AlterColumn<string>(
                name: "PORTFOLIO_NAME",
                table: "PORTFOLIO_MASTER",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "USER_MASTER_ID",
                table: "PORTFOLIO_MASTER",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "userMasterUSER_MASTER_ID",
            //    table: "PORTFOLIO_MASTER",
            //    type: "INTEGER",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_PORTFOLIO_MASTER_userMasterUSER_MASTER_ID",
            //    table: "PORTFOLIO_MASTER",
            //    column: "userMasterUSER_MASTER_ID");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_PORTFOLIO_MASTER_USER_MASTER_userMasterUSER_MASTER_ID",
            //    table: "PORTFOLIO_MASTER",
            //    column: "userMasterUSER_MASTER_ID",
            //    principalTable: "USER_MASTER",
            //    principalColumn: "USER_MASTER_ID");
        }
    }
}

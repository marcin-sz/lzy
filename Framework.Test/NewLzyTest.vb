﻿Imports LazyFramework.Data
Imports LazyFramework.Logging
Imports NUnit.Framework
Imports SqlServer

'Public Class SqlConnectionInfoBuilder
'    Implements IConnectionInfoBuilder

'    Public Function Build(conStr As String) As ServerConnectionInfo Implements IConnectionInfoBuilder.Build
'        Return New ServerConnectionInfo With {.UserName = "sa", .Password = "InfoTjenester88", .Database = "Hr", .Address = "13-testsql"}
'    End Function
'End Class


<TestFixture> Public Class NewLzyTest

    Public Shared Connection As New MSSqlServer.ServerConnectionInfo With {.Address = "13-testsql", .Database = "hr", .UserName = "sa", .Password = "supermann"}

    Private _MemoryLogger As MemoryLogger


    <SetUp> Public Sub SetUp()
        Runtime.Context.Current = New Runtime.WinThread
        LazyFramework.ClassFactory.Clear()

        Dim lazyFrameworkConfiguration As LazyFrameworkConfiguration = New LazyFrameworkConfiguration
        LazyFramework.ClassFactory.SetTypeInstance(Of ILazyFrameworkConfiguration)(lazyFrameworkConfiguration)

        lazyFrameworkConfiguration.LogLevel = 100
        lazyFrameworkConfiguration.EnableTiming = True
        

        _MemoryLogger = New LazyFramework.Logging.MemoryLogger
        LazyFramework.ClassFactory.SetTypeInstance(Of LazyFramework.Logging.ILogger)(_MemoryLogger)
        
    End Sub

    <TearDown> Public Sub Tear()
        LazyFramework.ClassFactory.Clear()
    End Sub

    <Test> Public Sub FillObject()

        Dim cmd As New CommandInfo

        Dim a = New With {.a = 123}

        cmd.CommandText = "select * from Hrunit where id = @Id"
        cmd.TypeOfCommand = CommandInfoCommandTypeEnum.Read
        cmd.Parameters.Add("Id", DbType.Int32)



        Dim ret As New DataObject
        ret.Id = 27
        Store.Exec(Connection, cmd, ret)
        Assert.AreEqual("Sigurd Brekkesen", ret.Name)

        Debug.Print(LazyFramework.Utils.ResponseThread.Current.Timer.Timings.Count.ToString)
        For Each t In LazyFramework.Utils.ResponseThread.Current.Timer.Timings
            Debug.Print(t.Key & t.Value.List(0))
        Next


    End Sub


    <Test> Public Sub SelectMany()

        Dim cmd As New CommandInfo
        cmd.CommandText = "select * from Hrunit"
        cmd.TypeOfCommand = CommandInfoCommandTypeEnum.Read
        cmd.Parameters.Add("Id", DbType.Int32, 1)

        Dim ret As New List(Of DataObject)

        Store.Exec(Connection, cmd, ret)

        Assert.AreNotEqual(0, ret.Count)
        'Assert.AreEqual("Petter Ekrann", ret.Name)
    End Sub

    <Test> Public Sub InheritedGenericListIsWorking()
        Dim cmd As New CommandInfo
        cmd.CommandText = "select * from Hrunit"
        cmd.TypeOfCommand = CommandInfoCommandTypeEnum.Read
        cmd.Parameters.Add("Id", DbType.Int32, 1)

        Dim ret As New DataObjectList

        Store.Exec(Connection, cmd, ret)

        Assert.AreNotEqual(0, ret.Count)
    End Sub


    <Test> Public Sub ListWithObjectsOfEntityBaseGetCorrectFillStatus()
        Dim cmd As New CommandInfo
        cmd.CommandText = "select * from Hrunit"
        cmd.TypeOfCommand = CommandInfoCommandTypeEnum.Read
        cmd.Parameters.Add("Id", DbType.Int32, 1)

        Dim ret As New DataObjectList

        Store.Exec(Connection, cmd, ret)

        Assert.AreNotEqual(0, ret.Count)
        Assert.AreEqual(ret(0).FillResult, FillResultEnum.DataFound)


    End Sub



    Public Class DataObjectList
        Inherits List(Of DataObject)

    End Class


    <Test> Public Sub FillerIsReused()

        Store.Fillers.Clear()

        'Hent mange
        Dim cmd As New CommandInfo
        cmd.CommandText = "select * from Hrunit"
        cmd.TypeOfCommand = CommandInfoCommandTypeEnum.Read
        Dim ret As New List(Of DataObject)


        Dim cmd2 As New CommandInfo
        cmd2.CommandText = "select * from Hrunit where id = @Id"
        cmd2.TypeOfCommand = CommandInfoCommandTypeEnum.Read
        cmd2.Parameters.Add("Id", DbType.Int32, 1)

        Dim data As New DataObject
        Store.Exec(Connection, cmd, ret)
        Store.Exec(Connection, cmd2, data)

        Assert.AreEqual(1, Store.Fillers.Count)

    End Sub

    <Test> Public Sub UseExpression()
        Dim cmd As New CommandInfo
        cmd.TypeOfCommand = CommandInfoCommandTypeEnum.Read

        'cmd.CommandQuery 

        Dim ret As New List(Of DataObject)

    End Sub


    <Test> Public Sub TestNullables()

        Dim o = New Nullable(Of Integer)(4)
        Dim o2 As Integer?
        Dim o3 As Object = New Nullable(Of Integer)
        Dim o4 As Integer? = 4


        'Debug.Print(o2.GetValueOrDefault.ToString)

        Assert.AreNotEqual(o, Nothing)
        Assert.AreEqual(o2, Nothing)
        Assert.AreEqual(o3, Nothing)
        Assert.AreNotEqual(o4, Nothing)

        Assert.IsTrue(o.GetType.IsValueType)
        Assert.IsFalse(o4.GetType.IsGenericType)



    End Sub



    <Test> Public Sub UseFillStatus()

        Dim cmd2 As New CommandInfo
        cmd2.CommandText = "select * from Hrunit where id = @Id"
        cmd2.TypeOfCommand = CommandInfoCommandTypeEnum.Read
        cmd2.Parameters.Add("Id", DbType.Int32, 27)

        Dim data As New FillStatus(Of DataObject)

        Store.Exec(Connection, cmd2, data)

        Assert.AreEqual(FillResultEnum.DataFound, data.FillResult)



    End Sub

    <Test> Public Sub TestingLog()

        Logger.Log(0, New GenericLogEvent("Start"))

        Dim cmd2 As New CommandInfo
        cmd2.CommandText = "select * from Hrunit where id = @Id"
        cmd2.TypeOfCommand = CommandInfoCommandTypeEnum.Read
        cmd2.Parameters.Add("Id", DbType.Int32, 1)


        Dim data As New DataObject
        Store.Exec(Connection, cmd2, data)

    End Sub

End Class


Public Class DataObject
    Inherits EntityBase

    
    Property Id As Integer
    Property Name As String
    Property Age As Integer
    Property BirthDay As DateTime

    Property Mail As List(Of Mail)
    Property IsSet As Boolean?

    
End Class

Public Class Mail
    Property PersonId As Integer
    Property Address As String
End Class

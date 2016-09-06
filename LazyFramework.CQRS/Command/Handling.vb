﻿Imports System.Reflection
Imports LazyFramework.CQRS.Security

Namespace Command
    Public Class Handling

        Private Shared _handlers As New Dictionary(Of Type, Func(Of Object, IAmACommand, ExecutionProfile))
        Private Shared _commadList As New Dictionary(Of String, Type)

        Public Shared Sub ClearMapping()
            _handlers = New Dictionary(Of Type, Func(Of Object, IAmACommand, ExecutionProfile))
            _commadList = New Dictionary(Of String, Type)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property CommandList As Dictionary(Of String, Type)
            Get
                Return _commadList
            End Get
        End Property


        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared ReadOnly Property AllHandlers() As Dictionary(Of Type, Func(Of Object, IAmACommand, ExecutionProfile))
            Get
                Return _handlers
            End Get
        End Property

        ''' <summary>
        ''' Adds a mapping to a function for the given command. 
        ''' </summary>
        ''' <typeparam name="TCommand"></typeparam>
        ''' <param name="run"></param>
        Public Shared Sub AddCommandHandler(Of TCommand As IAmACommand)(run As Func(Of Object, IAmACommand, ExecutionProfile))

            _handlers.Add(GetType(TCommand), run)
            _commadList.Add(GetType(TCommand).FullName, GetType(TCommand))
        End Sub


        ''' <summary>
        ''' Executes a command by finding the mapping to the type of command passed in. 
        ''' </summary>
        ''' <param name="command"></param>
        ''' <remarks>Any command can have only 1 handler. An exception will be thrown if there is found more than one for any given command. </remarks>
        Public Shared Function ExecuteCommand(profile As Object, command As IAmACommand) As Object

            Try
                Dim commandExecProfile = AllHandlers(command.GetType)(profile, command)

                If commandExecProfile.ActionIsAvailable IsNot Nothing AndAlso Not commandExecProfile.ActionIsAvailable.IsAvailable(CType(commandExecProfile, CommandExecutionBase).Entity) Then
                    Throw New ActionIsNotAvailableException(command, profile)
                End If

                If Not CanUserRunCommand(CType(commandExecProfile, CommandExecutionBase), CType(command, CommandBase)) Then
                    Throw New ActionSecurityAuthorizationFaildException(command, profile)
                End If

                If commandExecProfile.ValidateAction IsNot Nothing Then commandExecProfile.ValidateAction.InternalValidate(command)
                Dim commandResult = Transform.Handling.TransformResult(commandExecProfile, commandExecProfile.ActionHandler(command))
                command.SetResult(commandResult)
                Return commandResult
                command.ActionComplete()
            Catch ex As TargetInvocationException
                Logging.Log.Error(command, ex)
                Throw ex.InnerException
            Catch ex As Exception
                Logging.Log.Error(command, ex)
                Throw
            End Try
        End Function

        Public Shared Function CanUserRunCommand(profile As CommandExecutionBase, cmd As CommandBase) As Boolean
            If profile.ActionSecurity Is Nothing Then
                Return True
            End If

            Return profile.ActionSecurity.UserCanRunThisAction()

        End Function


    End Class

End Namespace

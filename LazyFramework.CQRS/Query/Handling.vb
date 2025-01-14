﻿Imports System.Reflection
Imports LazyFramework.CQRS.Monitor
Imports LazyFramework.CQRS.Security

Namespace Query
    Public Class Handling

        Private Shared _queryList As New Dictionary(Of String, Type)
        Public Shared ReadOnly Property QueryList As Dictionary(Of String, Type)
            Get
                Return _queryList
            End Get
        End Property


        ''Public Shared Sub AddQueryHandler(Of T As IAmAQuery)(handler As Func(Of T, Object))

        Public Shared Sub AddQueryHandler(Of TQuery As IAmAQuery, TBo, TDto)(handler As Func(Of Object, TQuery, QueryExecutionProfile(Of TQuery, TBo, TDto)))

            If _queryList.ContainsKey(GetType(TQuery).FullName) Then
                Throw New AllreadeyConfiguredException(GetType(TQuery))
            Else
                _queryList(GetType(TQuery).FullName) = GetType(TQuery)
            End If

            _handlers.Add(GetType(TQuery), New Func(Of Object, IAmAQuery, ExecutionProfile)(Function(o, a) handler(o, CType(a, TQuery))))
        End Sub

        Public Shared Sub ClearHandlers()
            _queryList = New Dictionary(Of String, Type)
            _handlers = New Dictionary(Of Type, Func(Of Object, IAmAQuery, ExecutionProfile))
        End Sub


        Private Delegate Function InternalHandler(query As Object) As Object

        Private Shared _handlers As New Dictionary(Of Type, Func(Of Object, IAmAQuery, ExecutionProfile))
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared ReadOnly Property Handlers() As Dictionary(Of Type, Func(Of Object, IAmAQuery, ExecutionProfile))
            Get
                Return _handlers
            End Get
        End Property



        Public Shared Function ExecuteQuery(profile As Object, q As IAmAQuery) As Object

            Dim handler As Func(Of Object, IAmAQuery, ExecutionProfile) = Nothing
            'Standard queryhandling. 1->1 mapping 
            If _handlers.TryGetValue(q.GetType, handler) Then

                Dim context = handler(profile, q)

                context.Start()
                context.Action = q

                If context.ActionSecurity IsNot Nothing AndAlso Not context.ActionSecurity.UserCanRunThisAction(q) Then
                    Dim actionSecurityAuthorizationFaildException As ActionSecurityAuthorizationFaildException = New ActionSecurityAuthorizationFaildException(q, profile)
                    Logging.Log.Error(context, actionSecurityAuthorizationFaildException)
                    Throw actionSecurityAuthorizationFaildException
                End If

                If context.ValidateAction IsNot Nothing Then context.ValidateAction.InternalValidate(q)
                Try
                    Dim result As Object = context.ActionHandler(q)
                    result = Transform.Handling.TransformResult(context, result)
                    context.Stopp()
                    Logging.Log.Context(context)

                    Return result

                Catch ex As TargetInvocationException
                    Logging.Log.Error(context, ex)
                    Throw ex.InnerException
                Catch ex As AggregateException
                    Logging.Log.Error(context, ex)
                    Throw
                Catch ex As Exception
                    Logging.Log.Error(context, ex)
                    Throw
                End Try
            End If

            Throw New NotSupportedException("Query handler not found")

        End Function



    End Class
End Namespace

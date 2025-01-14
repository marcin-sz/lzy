﻿
    Friend Class ListFiller
        Private ReadOnly _colName As String

        Public Sub New()
        End Sub

        Public Sub New(colName As String)
            _colName = colName
        End Sub

        Public Sub FillList(Of T As {New, IList})(filler As Store.FillObject, reader As IDataReader, data As FillStatus(Of T))
            'Data her er en ienumerable(of T)
            Dim counter As Integer = 0
            Dim listObjectType As Type = Nothing

            Dim type = data.Value.GetType
            While type IsNot Nothing
                If type.IsGenericType Then
                    listObjectType = type.GetGenericArguments(0)
                    Exit While
                End If
                type = type.BaseType
            End While

            If listObjectType Is Nothing Then
                Throw New NotSupportedException("List must inherit from System.Collection.Generic.List<type>")
            End If

            While reader.Read
                counter += 1
                Dim toFill = Activator.CreateInstance(listObjectType)
                Store.FillData(reader, filler, toFill)
                If TypeOf (toFill) Is EntityBase Then
                    DirectCast(toFill, EntityBase).FillResult = FillResultEnum.DataFound
                    DirectCast(toFill, EntityBase).Loaded = Now.Ticks
                End If
                data.Value.Add(toFill)
            End While


            Select Case counter
                Case 0
                    data.FillResult = FillResultEnum.NoData
                Case 1
                    data.FillResult = FillResultEnum.DataFound
                Case Else
                    data.FillResult = FillResultEnum.MultipleLinesFound
            End Select

        End Sub


        Public Sub FillListForValueType(Of T As Structure)(reader As IDataReader, data As ICollection(Of T))
            While reader.Read
                data.Add(CType(reader(_colName), T))
            End While
        End Sub
    End Class

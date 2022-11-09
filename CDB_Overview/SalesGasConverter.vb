Imports System.Globalization

'add by yutecxa 2022-11-09
Public Class SalesGasConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If IsNumeric(value) Then
            Return Format(CSng(value) * 100, "#.##")
        Else
            Return "N/A"
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Dim objTypeConverter As System.ComponentModel.TypeConverter = System.ComponentModel.TypeDescriptor.GetConverter(targetType)
        Dim objReturnValue As Object = Nothing

        If objTypeConverter.CanConvertFrom(value.[GetType]()) Then
            objReturnValue = objTypeConverter.ConvertFrom(value)
        End If

        Return objReturnValue
    End Function
End Class

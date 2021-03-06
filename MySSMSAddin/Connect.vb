Imports System
Imports Microsoft.VisualStudio.CommandBars
Imports Extensibility
Imports EnvDTE
Imports EnvDTE80
Imports System.Reflection

Public Class Connect
	
    Implements IDTExtensibility2
	Implements IDTCommandTarget

    Private _DTE2 As DTE2
    Private _DTE As DTE
    Private _addInInstance As AddIn

    Private _CommandEvents As CommandEvents

    Private _CommandBarControl As CommandBarControl

    Private Const COMMAND_NAME As String = "MySSMSAddinCommand"

    '''<summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
    Public Sub New()

    End Sub

    '''<summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
    '''<param name='application'>Root object of the host application.</param>
    '''<param name='connectMode'>Describes how the Add-in is being loaded.</param>
    '''<param name='addInInst'>Object representing this Add-in.</param>
    '''<remarks></remarks>
    Public Sub OnConnection(ByVal application As Object, ByVal connectMode As ext_ConnectMode, ByVal addInInst As Object, ByRef custom As Array) Implements IDTExtensibility2.OnConnection

        _DTE2 = CType(application, DTE2)
        _DTE = CType(application, DTE)
        _addInInstance = CType(addInInst, AddIn)

        ' pop up a message to prove it's working
        'System.Windows.Forms.MessageBox.Show("The OnConnection method of MySSMSAddin has been called", "MySSMSAddin", System.Windows.Forms.MessageBoxButtons.OK)

        ' list all of the commands
        'For Each cmd As EnvDTE.Command In _DTE.Commands
        '    Debug.WriteLine(String.Format("Name = {0} | GUID = {1} | ID = {2}", cmd.Name, cmd.Guid, cmd.ID))
        'Next

        ' get the events for the command we're interested in (the GUID comes from the output of the previous debug command)
        ' NOTE: if the _CommandEvents object goes out of scope then the handler will not longer be attached to the event,
        ' so it must be a private class-level declaration rather than a local one.
        _CommandEvents = _DTE.Events.CommandEvents("{54692960-56BC-4989-B5D3-92C47A513E8D}", 1)

        
	End Sub

    '''<summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
    '''<param name='disconnectMode'>Describes how the Add-in is being unloaded.</param>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnDisconnection(ByVal disconnectMode As ext_DisconnectMode, ByRef custom As Array) Implements IDTExtensibility2.OnDisconnection
        Try

            ' check whether the control in the tools menu is there
            If Not (_CommandBarControl Is Nothing) Then
                ' delete the menu item
                _CommandBarControl.Delete()
            End If
        Catch
        End Try
    End Sub

    '''<summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification that the collection of Add-ins has changed.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnAddInsUpdate(ByRef custom As Array) Implements IDTExtensibility2.OnAddInsUpdate
    End Sub

    '''<summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnStartupComplete(ByRef custom As Array) Implements IDTExtensibility2.OnStartupComplete
        Dim myCommand As Command = Nothing

        ' -----------------------------------
        ' 1. Check whether the command exists
        ' -----------------------------------

        ' try to retrieve the command, in case it was already created
        Try
            myCommand = _DTE.Commands.Item(_addInInstance.ProgID & "." & COMMAND_NAME)
        Catch
            ' this just means the command wasn't found
        End Try

        ' ----------------------------------
        ' 2. Create the command if necessary
        ' ----------------------------------

        If myCommand Is Nothing Then
            myCommand = _DTE.Commands.AddNamedCommand(_addInInstance, COMMAND_NAME, "MySSMSAddin MenuItem", "Tooltip for your command", True, 0, Nothing, vsCommandStatus.vsCommandStatusSupported Or vsCommandStatus.vsCommandStatusEnabled)
        End If

        ' ------------------------------------------------------------------------------------
        ' 3. Get the name of the tools menu (may not be called "Tools" if we're not in English
        ' ------------------------------------------------------------------------------------

        Dim toolsMenuName As String
        Try

            ' If you would like to move the command to a different menu, change the word "Tools" to the 
            ' English version of the menu. This code will take the culture, append on the name of the menu
            ' then add the command to that menu. You can find a list of all the top-level menus in the file
            ' CommandBar.resx.
            Dim resourceManager As System.Resources.ResourceManager = New System.Resources.ResourceManager("MySSMSAddin.CommandBar", System.Reflection.Assembly.GetExecutingAssembly())

            Dim cultureInfo As System.Globalization.CultureInfo = New System.Globalization.CultureInfo(_DTE2.LocaleID)
            toolsMenuName = resourceManager.GetString(String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools"))

        Catch e As Exception
            'We tried to find a localized version of the word Tools, but one was not found.
            '  Default to the en-US word, which may work for the current culture.
            toolsMenuName = "Tools"
        End Try

        ' ---------------------
        ' 4. Get the Tools menu
        ' ---------------------

        Dim commandBars As CommandBars = DirectCast(_DTE.CommandBars, CommandBars)
        Dim toolsCommandBar As CommandBar = commandBars.Item(toolsMenuName)

        ' -------------------------------------------------
        ' 5. Create the command bar control for the command
        ' -------------------------------------------------

        Try
            'Find the appropriate command bar on the MenuBar command bar:
            _CommandBarControl = DirectCast(myCommand.AddControl(toolsCommandBar, toolsCommandBar.Controls.Count + 1), CommandBarControl)
            _CommandBarControl.Caption = "MySSMSAddin"
        Catch argumentException As System.ArgumentException
            'If we are here, then the exception is probably because a command with that name
            '  already exists. If so there is no need to recreate the command and we can 
            '  safely ignore the exception.
        End Try
    End Sub

    '''<summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnBeginShutdown(ByRef custom As Array) Implements IDTExtensibility2.OnBeginShutdown
    End Sub
	
    '''<summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
    '''<param name='commandName'>The name of the command to determine state for.</param>
    '''<param name='neededText'>Text that is needed for the command.</param>
    '''<param name='status'>The state of the command in the user interface.</param>
    '''<param name='commandText'>Text requested by the neededText parameter.</param>
    '''<remarks></remarks>
    Public Sub QueryStatus(ByVal commandName As String, ByVal neededText As vsCommandStatusTextWanted, ByRef status As vsCommandStatus, ByRef commandText As Object) Implements IDTCommandTarget.QueryStatus
        If neededText = vsCommandStatusTextWanted.vsCommandStatusTextWantedNone Then
            If commandName = _addInInstance.ProgID & "." & COMMAND_NAME Then
                status = CType(vsCommandStatus.vsCommandStatusEnabled + vsCommandStatus.vsCommandStatusSupported, vsCommandStatus)
            Else
                status = vsCommandStatus.vsCommandStatusUnsupported
            End If
        End If
    End Sub

    '''<summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
    '''<param name='commandName'>The name of the command to execute.</param>
    '''<param name='executeOption'>Describes how the command should be run.</param>
    '''<param name='varIn'>Parameters passed from the caller to the command handler.</param>
    '''<param name='varOut'>Parameters passed from the command handler to the caller.</param>
    '''<param name='handled'>Informs the caller if the command was handled or not.</param>
    '''<remarks></remarks>
    Public Sub Exec(ByVal commandName As String, ByVal executeOption As vsCommandExecOption, ByRef varIn As Object, ByRef varOut As Object, ByRef handled As Boolean) Implements IDTCommandTarget.Exec
        handled = False
        If executeOption = vsCommandExecOption.vsCommandExecOptionDoDefault Then
            If commandName = _addInInstance.ProgID & "." & COMMAND_NAME Then

                ' get windows2 interface
                Dim MyWindow As Windows2 = CType(_DTE2.Windows, Windows2)

                ' get current assembly
                Dim asm As Assembly = System.Reflection.Assembly.GetExecutingAssembly

                ' create the window
                Dim MyControl As Object = Nothing
                Dim toolWindow As Window = MyWindow.CreateToolWindow2(_addInInstance, asm.Location, "MySSMSAddin.MyAddinWindow", "MySMSAddin Window", "{c5A3cf1C-1B20-71Fa-858F-F58b3f12C1c1}", MyControl)
                toolWindow.Visible = True

                handled = True

            End If
        End If
    End Sub
End Class

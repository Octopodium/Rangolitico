using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.DualShock;
public class InputController : MonoBehaviour {
    public PlayerInputManager playerInputManager; // Gerencia os inputs dos jogadores locais
    public Actions actions;

    public Color anglerColor, heaterColor;
    public PlayerInput player1Input, player2Input;
    public InputDevice player1Device, player2Device;

    public Action<InputAction.CallbackContext, QualPlayer> OnInputTriggered;

    void Awake() {
        InputUser.onChange += OnInputUserChanged;
    }

    void Start() {
        GameManager.instance.OnTrocarControle += AtualizarCorzinhaControle;
    }

    void OnDestroy() {
        InputUser.onChange -= OnInputUserChanged;
        actions?.Disable();
    }

    public void Inicializar() {
        actions = new Actions();
        actions.Enable();
    }

    public void Desabilitar() {
        actions.Disable();
    }

    #region Input e Devices

    public void ConfigurarInputs() {
        player1Input.gameObject.SetActive(true);
        player1Input.onActionTriggered += ctx => HandleOnInputTriggered(ctx, QualPlayer.Player1);
        player1Input.onDeviceLost += ctx => OnDeviceLost(player1Input, QualPlayer.Player1);

        if (GameManager.instance.modoDeJogo == ModoDeJogo.MULTIPLAYER_LOCAL) {
            player2Input.gameObject.SetActive(true);
            player2Input.onActionTriggered += ctx => HandleOnInputTriggered(ctx, QualPlayer.Player2);
            player2Input.onDeviceLost += ctx => OnDeviceLost(player2Input, QualPlayer.Player2);

            CadastrarDevices();
        } else {
            Destroy(player2Input.gameObject);
            player2Input = null;

            AtualizarCorzinhaControle(QualPlayer.Player1);
        }
    }
    
    QualPlayer GetQualPlayerFromInputUser(InputUser inputUser) {
        if (inputUser == player1Input.user) {
            return QualPlayer.Player1;
        } else if (inputUser == player2Input?.user) {
            return QualPlayer.Player2;
        }
        return QualPlayer.Player1; // Ou outro valor padrão
    }


    protected void OnInputUserChanged(InputUser inputUser, InputUserChange change, InputDevice device) {
        bool paired = change == InputUserChange.DevicePaired;
        bool unpaired = change == InputUserChange.DeviceUnpaired;

        if (!paired && !unpaired) return;

        QualPlayer player = GetQualPlayerFromInputUser(inputUser);
        AtualizarCorzinhaControle(player);
    }

    
    protected void OnDeviceLost(PlayerInput playerInput, QualPlayer qualPlayer) {
        Debug.Log($"Dispositivo perdido para {qualPlayer}: {playerInput.currentControlScheme}");

        if (qualPlayer == QualPlayer.Player1) {
            player1Device = null;
            player1Input.user.UnpairDevices();
            player1Input.DeactivateInput();
        } else if (qualPlayer == QualPlayer.Player2) {
            player2Device = null;
            player2Input.user.UnpairDevices();
            player2Input.DeactivateInput();
        }

        UIConexaoInGame.instancia.SetConectando(qualPlayer);
        actions.Player.Get().actionTriggered += OuveAcoesParaCadastrarDevices;
    }

    protected QualPlayer GetQualPlayerTratado(QualPlayer qualPlayer) {
        if (GameManager.instance.modoDeJogo == ModoDeJogo.SINGLEPLAYER) {
            return GameManager.instance.playerAtual;
        } else if (GameManager.instance.modoDeJogo == ModoDeJogo.MULTIPLAYER_ONLINE) {
            return QualPlayer.Player1;
        }
        return qualPlayer;

    }

    protected void HandleOnInputTriggered(InputAction.CallbackContext ctx, QualPlayer qualPlayer) {
        qualPlayer = GetQualPlayerTratado(qualPlayer);

        if (OnInputTriggered != null) {
            OnInputTriggered(ctx, qualPlayer);
        }
    }

    public PlayerInput GetPlayerInput(Player player) {
        return GetPlayerInput(player.qualPlayer);
    }

    public PlayerInput GetPlayerInput(QualPlayer qualPlayer) {
        if (GameManager.instance.modoDeJogo == ModoDeJogo.MULTIPLAYER_LOCAL) {
            return (qualPlayer == QualPlayer.Player1) ? player1Input : player2Input;
        } else {
            return (qualPlayer == GameManager.instance.playerAtual) ? player1Input : null;
        }
    }

    protected void CadastrarDevices() {
        if (GameManager.instance.modoDeJogo != ModoDeJogo.MULTIPLAYER_LOCAL) return;

        player1Device = null;
        player2Device = null;

        if (player1Input.user.valid) player1Input.user.UnpairDevices();
        if (player2Input.user.valid) player2Input.user.UnpairDevices();

        player1Input.DeactivateInput();
        player2Input.DeactivateInput();

        UIConexaoInGame.instancia.SetConectando(QualPlayer.Player1);
        UIConexaoInGame.instancia.SetConectando(QualPlayer.Player2);

        actions.Player.Get().actionTriggered += OuveAcoesParaCadastrarDevices;
    }

    protected void OuveAcoesParaCadastrarDevices(InputAction.CallbackContext ctx) {
        InputDevice device = ctx.control.device;
        if (device == null) return;

        if (player1Device != null && player2Device != null) {
            actions.Player.Get().actionTriggered -= OuveAcoesParaCadastrarDevices;
            return;
        }

        if (player1Device == null) {
            CadastrarDevice(QualPlayer.Player1, device);
        } else if (player2Device == null && player1Device != device) {
            CadastrarDevice(QualPlayer.Player2, device);
        }
    }

    protected void CadastrarDevice(QualPlayer player, InputDevice device) {
        if (device == null) return;

        bool isPlayer1 = player == QualPlayer.Player1;

        if (isPlayer1) {
            if (player2Device != null && player2Device == device) {
                // Se o dispositivo já estiver registrado para o Player2, não faz nada
                return;
            }

            player1Device = device;
        } else {
            if (player1Device != null && player1Device == device) {
                // Se o dispositivo já estiver registrado para o Player1, não faz nada
                return;
            }

            player2Device = device;
        }

        PlayerInput playerInput = isPlayer1 ? player1Input : player2Input;
        playerInput.user.UnpairDevices();
        InputUser.PerformPairingWithDevice(device, playerInput.user);

        // O Input System não reconhece automaticamente o esquema de controle, então é necessário definir manualmente... (??????)
        string controlScheme = GetControlSchemeName(device);
        playerInput.SwitchCurrentControlScheme(controlScheme, device);
        playerInput.ActivateInput();

        UIConexaoInGame.instancia.SetConectado(player);

        Debug.Log($"Dispositivo {device.displayName} cadastrado para o {player}. Device: {device}. Control Scheme: {controlScheme}");
    }

    string GetControlSchemeName(InputDevice device) {
        switch (device) {
            case Gamepad:
                return "Gamepad";
            case Keyboard:
                return "Keyboard&Mouse";
            case Touchscreen:
                return "Touch";
            case Joystick:
                return "Joystick";
            default:
                return "Unknown";
        }
    }

    #endregion

    void AtualizarCorzinhaControle(QualPlayer playerAtual) {
        if (GameManager.instance == null) return;

        QualPersonagem personagem = GameManager.instance.GetQualPersonagem(playerAtual);
        PlayerInput playerInput = GetPlayerInput(playerAtual);

        if (playerInput == null) return;

        foreach (var device in playerInput.devices) {
            if (device is DualShockGamepad dualShockGamepad) {
                if (personagem == QualPersonagem.Angler) {
                    dualShockGamepad.SetLightBarColor(anglerColor);
                } else if (personagem == QualPersonagem.Heater) {
                    dualShockGamepad.SetLightBarColor(heaterColor);
                }
            }
        }
    }

}

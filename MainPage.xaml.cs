using System.Globalization;

namespace Calculatrice;

public partial class MainPage : ContentPage
{
    private string _current = "0";      // nombre en cours de saisie
    private decimal? _firstOperand;     // premier nombre de l'opération
    private string? _operator;          // opérateur choisi (+, −, ×, ÷)
    private bool _startNewEntry;        // true : le prochain chiffre remplace l'affichage
    private bool _hasError;             // true après une erreur (ex. division par zéro)

    public MainPage()
    {
        InitializeComponent();
    }

    // ---------- Chiffres ----------
    private void OnDigitClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button) return;
        if (_hasError) Reset();

        if (_startNewEntry || _current == "0")
        {
            _current = button.Text;
            _startNewEntry = false;
        }
        else if (_current == "-0")
        {
            _current = "-" + button.Text;
        }
        else if (_current.Length < 15)
        {
            _current += button.Text;
        }

        Refresh();
    }

    // ---------- Virgule (point décimal) ----------
    private void OnDecimalClicked(object? sender, EventArgs e)
    {
        if (_hasError) Reset();

        if (_startNewEntry)
        {
            _current = "0.";
            _startNewEntry = false;
        }
        else if (!_current.Contains('.'))
        {
            _current += ".";
        }

        Refresh();
    }

    // ---------- Opérateurs (+, −, ×, ÷) ----------
    private void OnOperatorClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || _hasError) return;

        // Si une opération est déjà en attente, on la calcule d'abord (enchaînement)
        if (_firstOperand.HasValue && _operator != null && !_startNewEntry)
        {
            if (!TryCalculate(_firstOperand.Value, Parse(_current), _operator, out decimal intermediate))
            {
                ShowError();
                return;
            }
            _current = Format(intermediate);
            Refresh();
        }

        _firstOperand = Parse(_current);
        _operator = button.Text;
        _startNewEntry = true;
        ExpressionLabel.Text = $"{Format(_firstOperand.Value)} {_operator}";
    }

    // ---------- Égal ----------
    private void OnEqualsClicked(object? sender, EventArgs e)
    {
        if (_hasError || _firstOperand is null || _operator is null) return;

        decimal second = Parse(_current);
        string expression = $"{Format(_firstOperand.Value)} {_operator} {Format(second)} =";

        if (!TryCalculate(_firstOperand.Value, second, _operator, out decimal result))
        {
            ShowError();
            return;
        }

        _current = Format(result);
        ExpressionLabel.Text = expression;
        _firstOperand = null;
        _operator = null;
        _startNewEntry = true;
        Refresh();
    }

    // ---------- Effacer (C) ----------
    private void OnClearClicked(object? sender, EventArgs e)
    {
        Reset();
    }

    // ---------- Changement de signe (±) ----------
    private void OnSignClicked(object? sender, EventArgs e)
    {
        if (_hasError || _current == "0") return;

        _current = _current.StartsWith('-') ? _current[1..] : "-" + _current;
        Refresh();
    }

    // ---------- Pourcentage (%) ----------
    private void OnPercentClicked(object? sender, EventArgs e)
    {
        if (_hasError) return;

        _current = Format(Parse(_current) / 100m);
        _startNewEntry = true;
        Refresh();
    }

    // ---------- Méthodes utilitaires ----------
    private static bool TryCalculate(decimal a, decimal b, string op, out decimal result)
    {
        result = 0;
        try
        {
            switch (op)
            {
                case "+": result = a + b; return true;
                case "−": result = a - b; return true;
                case "×": result = a * b; return true;
                case "÷":
                    if (b == 0) return false;   // division par zéro
                    result = a / b;
                    return true;
            }
        }
        catch (OverflowException)
        {
            return false;                       // nombre trop grand
        }
        return false;
    }

    private static decimal Parse(string text)
    {
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value)
            ? value
            : 0m;
    }

    private static string Format(decimal value)
    {
        return Math.Round(value, 10).ToString("0.##########", CultureInfo.InvariantCulture);
    }

    private void Refresh()
    {
        ResultLabel.Text = _current;
    }

    private void ShowError()
    {
        _hasError = true;
        _firstOperand = null;
        _operator = null;
        ExpressionLabel.Text = string.Empty;
        ResultLabel.Text = "Erreur";
    }

    private void Reset()
    {
        _current = "0";
        _firstOperand = null;
        _operator = null;
        _startNewEntry = false;
        _hasError = false;
        ExpressionLabel.Text = string.Empty;
        ResultLabel.Text = "0";
    }
}

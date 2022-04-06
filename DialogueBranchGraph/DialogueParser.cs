using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RedBlueGames.Tools.TextTyper;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class DialogueParser : MonoBehaviour
{
    public xDialogueGraph graph;
    public GameObject DialogueBar;
    public GameObject ChoiceBar;
    public Transform ChoiceContainer;
    public GameObject ChoicePrefab;
    [SerializeField] private TextTyper typer;
    public TextMeshProUGUI tmpro_dialogue;
    public TextMeshProUGUI tmpro_name;
    private UIInputHandler inputHandler;
    [SerializeField] private float baseDefaultHeight = 128;
    [SerializeField] private float baseLineHeight = 20;
    [SerializeField] private float baseMaxHeight = 188;
    [SerializeField] private RectTransform baseTransform;
    [SerializeField] private Image namePlateRenderer;
    [SerializeField] private AutoLayoutColumn autoLayoutColumn;
    public TextMeshProUGUI tmpro_question;
    private string previousCharacter = "";
    private bool isChoosing = false;
    private List<string> seenChoices = new List<string>();

    private void Awake() {
        inputHandler = UIInputHandler.instance;
    }

    //Debug Only
    private void Start() {
        StartDialogue();
    }

    private void SkipText()
    {
        if(isChoosing) return;

        //Skip text or go to the next node
        if(typer.IsTyping)
        {
            typer.Skip();
        } else {
            NextNode();
        }
    }

    public void StartDialogue() {
        //Reset choices
        seenChoices.Clear();

        //Go to first node in graph
        foreach (xBaseNode b in graph.nodes)
        {
            if(b.GetString() == "Start")
            {
                graph.current = b;
                break;
            }
        }

        NextNode();
    }

    private void CreateNewBox()
    {
        var node = graph.current as xDialogueNode;

        //Set name
        var name = node.character.CharacterName;

        //If new character, popup animation
        if(name != previousCharacter)
        {
            var scale = DialogueBar.transform.localScale;
            var startScale = scale;
            startScale.y = 0;

            DialogueBar.transform.localScale = startScale;
            DialogueBar.transform.DOScale(scale, 0.3f).SetEase(Ease.OutBack);
        }

        tmpro_name.text = name;
        previousCharacter = name;

        //Set plate color
        namePlateRenderer.color = node.character.characterColor;

        var txtInfo = tmpro_dialogue.GetTextInfo(node.dialogueText);
        Debug.Log(baseDefaultHeight + baseLineHeight * (txtInfo.lineCount));

        baseTransform.sizeDelta = new Vector2(baseTransform.sizeDelta.x, baseDefaultHeight + baseLineHeight * (txtInfo.lineCount));

        typer.TypeText(node.dialogueText,.02f);
    }
    private void DisplayChoices()
    {
        isChoosing = true;

        var node = graph.current as xDialogueBranchNode;

        tmpro_question.text = node.dialogueText;
        
        autoLayoutColumn.UpdateColumns(node.branches.Count);

        //Choice index
        int i = 0;

        foreach(var choice in node.branches)
        {
            var choiceObject = Instantiate(ChoicePrefab,ChoiceContainer);
            choiceObject.GetComponentInChildren<TextMeshProUGUI>().text = choice.Value;

            choiceObject.transform.localScale = Vector3.zero;
            choiceObject.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetDelay(i * 0.1f);

            //If choice already seen, grey it out
            if(seenChoices.Contains(choice.Key))
            {
                Color greyedOut;
                ColorUtility.TryParseHtmlString("#9D9D9D", out greyedOut);
                choiceObject.GetComponentInChildren<Image>().color = greyedOut;
            }

            //Add listener to button click
            choiceObject.GetComponent<Button>().onClick.AddListener(delegate{ChoiceSelect(choice.Key);});

            i++;
        }
    }

    private void ChoiceSelect(string index)
    {
        //Remove All Listeners
        foreach(var button in ChoiceContainer.GetComponentsInChildren<Button>())
        {
            button.onClick.RemoveAllListeners();
            Destroy(button.gameObject);
        }

        //Add choice to seen dictionary
        seenChoices.Add(index);

        isChoosing = false;
        NextNode(index);
    }

    private void NextNode(string portFieldName = "")
    {
        foreach (NodePort p in graph.current.Ports)
        {
            if((portFieldName == "" && p.fieldName == "exit") || (p.fieldName == portFieldName))
            {
                graph.current = p.Connection.node as xBaseNode;
                break;
            }
        }

        var nodeType = graph.current.GetType();

        switch(graph.current)
        {
            case xDialogueNode s:
                DialogueBar.SetActive(true);
                ChoiceBar.SetActive(false);
                CreateNewBox();
                break;
            case xDialogueBranchNode c:
                DisplayChoices();
                DialogueBar.SetActive(false);
                ChoiceBar.SetActive(true);
                break;
        }
    }


    private void OnEnable() {
        inputHandler.UIControls.UI.TextSkip.performed += context => SkipText();
    }

    private void OnDisable() {
        inputHandler.UIControls.UI.TextSkip.performed -= context => SkipText();
    }
}

//Create Focus Point
//Move the character relative to focus and distance variable
//First person
//Camera LookAt Focus
//Create UI
//Show text dynamic, with simple sound
//Skip appearing text
//Go to next node
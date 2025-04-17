using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using HisaGames.TransformSetting;
using HisaGames.CutsceneManager;
using HisaGames.Props;
using HisaGames.Character;

namespace HisaGames.Cutscene
{
    public class EcCutscene : MonoBehaviour
    {
        [System.Serializable]
        public class CharacterData
        {
            [Tooltip("Name of the character.")]
            public string name;

            [Tooltip("Identifier for the initial transform configuration.")]
            public string initialTransformID;

            [Tooltip("Identifier for the final transform configuration.")]
            public string finalTransformID;

            [Tooltip("Name of the sprite to use for this character.")]
            public string spriteString;
        }

        [System.Serializable]
        public class PropsData
        {
            [Tooltip("Name of the prop.")]
            public string name;

            [Tooltip("Target position of the prop in the scene.")]
            public Vector3 position;

            [Tooltip("Time speed for the prop to fade in and become visible.")]
            public float fadeSpeed;
        }

        [System.Serializable]
        public class CSUnityEvent : UnityEvent { }

        [System.Serializable]
        public class CutsceneData
        {
            [Header("Cutscene Data")]
            [Tooltip("Name of the cutscene.")]
            public string name;

            [Tooltip("Array of character data for the cutscene.")]
            public CharacterData[] charactersData;

            [Tooltip("Array of props data for the cutscene.")]
            public PropsData[] propsData;

            [Tooltip("Name displayed in the chat.")]
            public string nameString;

            [Tooltip("Chat text displayed during the cutscene.")]
            [TextArea] public string chatString;

            [Header("Cutscene Event")]
            [Tooltip("Event triggered before the cutscene starts.")]
            public CSUnityEvent cutscenePreEvent;

            [Tooltip("Event triggered after the cutscene ends.")]
            public CSUnityEvent cutscenePostEvent;
        }

        [SerializeField]
        [Tooltip("Array of cutscene data for the sequence.")]
        CutsceneData[] cutsceneData;

        [Tooltip("Index of the currently active cutscene.")]
        public int currentID;

        [Header("Cutscene Settings")]
        [HideInInspector]
        [Tooltip("Timer for auto-playing the next cutscene.")]
        float autoplayTime;

        [Header("Other Settings (no need to change)")]
        [SerializeField]
        [Tooltip("Text field for character names.")]
        Text charaNameText;

        [SerializeField]
        [Tooltip("Text field for chat text.")]
        Text chatText;

        [Tooltip("Full string of the chat text for the current cutscene.")]
        string chatTextString;

        [Tooltip("Delay timer between typing each character in the chat.")]
        float typingTimer;

        [Tooltip("Indicates whether typing animation is currently active.")]
        bool startTyping;

        [Tooltip("Array of active props data for the current cutscene.")]
        PropsData[] activePropsData;

        void Start()
        {
            StartCutscene();
        }

        /// <summary>
        /// Initializes cutscene settings and starts the first cutscene.
        /// </summary>
        public void StartCutscene()
        {
            currentID = 0;
            autoplayTime = EcCutsceneManager.instance.autoplayTime;
            startTyping = false;
            typingTimer = EcCutsceneManager.instance.chatTypingDelay;

            PlayCutscene();
        }

        /// <summary>
        /// Handles updates for auto-playing cutscenes, typing animation, and character/prop states.
        /// </summary>
        void Update()
        {
            AutoPlayingCutscene();

            if (startTyping)
                StartTypingAnimation(chatText, chatTextString);

            for (int i = 0; i < cutsceneData[currentID].charactersData.Length; i++)
            {
                string tempName = cutsceneData[currentID].charactersData[i].name;
                EcCharacter character = EcCutsceneManager.instance.getCharacterObject(tempName);
                character.CheckingCharacterState();
            }

            if (activePropsData != null)
            {
                for (int i = 0; i < activePropsData.Length; i++)
                {
                    EcProps props = EcCutsceneManager.instance.getPropObject(activePropsData[i].name);
                    props.PropUpdate();
                }
            }
        }

        /// <summary>
        /// Plays the current cutscene.
        /// </summary>
        void PlayCutscene()
        {
            InvokePreEvent();
            PlayChatTypingAnimation();

            ShowingCurrentCharacters();

            ClearPreviousProps();
            ShowingCurrentProps();
        }

        /// <summary>
        /// Invokes the pre-event of the current cutscene.
        /// </summary>
        void InvokePreEvent()
        {
            if (cutsceneData[currentID].cutscenePreEvent != null)
            {
                cutsceneData[currentID].cutscenePreEvent.Invoke();
            }
        }

        /// <summary>
        /// Invokes the post-event of the current cutscene.
        /// </summary>
        void InvokePostEvent()
        {
            if (cutsceneData[currentID].cutscenePostEvent != null)
            {
                cutsceneData[currentID].cutscenePostEvent.Invoke();
            }
        }

        /// <summary>
        /// Plays the chat typing animation for the current cutscene.
        /// </summary>
        void PlayChatTypingAnimation()
        {
            //Play Chat Text Typing Animation on Cutscene
            chatText.text = "";
            chatTextString = cutsceneData[currentID].chatString;
            //CutsceneManager.instance.audioSource.Play();
            startTyping = true;

            charaNameText.text = cutsceneData[currentID].nameString;
            if (charaNameText.text == "")
                charaNameText.transform.parent.gameObject.SetActive(false);
            else
                charaNameText.transform.parent.gameObject.SetActive(true);
        }

        /// <summary>
        /// Displays the current characters and sets their animations and transforms.
        /// </summary>
        void ShowingCurrentCharacters()
        {
            //Showing & Play Character Animation on Movement on Cutscene
            for (int i = 0; i < cutsceneData[currentID].charactersData.Length; i++)
            {
                CharacterData tempCharaData = cutsceneData[currentID].charactersData[i];
                EcCharacter character = EcCutsceneManager.instance.getCharacterObject(tempCharaData.name);

                if (character != null)
                {
                    character.transform.gameObject.SetActive(true);

                    //Checking Chara Transform Position, Rotation, Scale Config ------------
                    if (tempCharaData.initialTransformID != "" && tempCharaData.finalTransformID != "")
                    {
                        EcTransformSetting charaInitialTransform
                            = EcCutsceneManager.instance.getCharaTransformSetting(tempCharaData.initialTransformID);

                        EcTransformSetting charaFinalTransform
                            = EcCutsceneManager.instance.getCharaTransformSetting(tempCharaData.finalTransformID);

                        if (charaInitialTransform != null)
                        {
                            //Checking & Setting Initial Transform Position, Rotation, Scale Config
                            character.transform.localRotation = Quaternion.Euler(charaInitialTransform.rotation);
                            character.transform.localScale = charaInitialTransform.scale;
                            character.transform.position = charaInitialTransform.position;

                            //Checking & Setting Final Transform Position, Rotation, Scale Config
                            if (charaFinalTransform != null)
                            {
                                character.SetCharacterMove(
                                    charaFinalTransform.position,
                                    charaFinalTransform.rotation,
                                    charaFinalTransform.scale
                                );
                            }
                        }
                        else
                        {
                            Debug.Log("There are no transform setting named with " + tempCharaData.initialTransformID);
                        }
                    }
                    //-----------------------------------------------------------------------

                    //Checking & Setting Sprite Config --------------------------------------
                    if (tempCharaData.spriteString != "")
                        character.ChangeSpriteByName(tempCharaData.spriteString);

                    if (character.name == cutsceneData[currentID].nameString)
                        character.spriteRenderer.color = Color.white; //set character sprite normal
                    else
                        character.spriteRenderer.color = Color.gray; //set character sprite normal
                                                                     //-----------------------------------------------------------------------
                }
                else
                {
                    Debug.Log("There are no characters named with " + tempCharaData.name);
                }
            }
        }

        /// <summary>
        /// Clears the props from the previous cutscene if they are still visible in the scene.
        /// </summary>
        void ClearPreviousProps()
        {
            //Clearing the previous props if still showed on scene
            if (activePropsData != null)
            {
                for (int i = 0; i < activePropsData.Length; i++)
                {
                    EcProps props = EcCutsceneManager.instance.getPropObject(activePropsData[i].name);

                    if (props != null)
                    {
                        props.transform.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.Log("There are no properties named with " + activePropsData[i].name);
                    }
                }
            }
        }

        /// <summary>
        /// Displays and sets up the props for the current cutscene.
        /// </summary>
        void ShowingCurrentProps()
        {
            //Showing Props on Cutscene
            activePropsData = cutsceneData[currentID].propsData;
            for (int i = 0; i < activePropsData.Length; i++)
            {
                EcProps props = EcCutsceneManager.instance.getPropObject(activePropsData[i].name);

                if (props != null)
                {
                    props.transform.gameObject.SetActive(true);
                    props.transform.position = activePropsData[i].position;
                    props.transform.localScale = Vector3.zero;
                    props.SetVisibility(activePropsData[i].fadeSpeed);
                }
                else
                {
                    Debug.Log("There are no properties named with " + activePropsData[i].name);
                }
            }
        }

        /// <summary>
        /// Displays chat text one character at a time to simulate typing animation.
        /// </summary>
        /// <param name="chatText">The UI text component where the typing effect is shown.</param>
        /// <param name="stringResult">The full string to display with the typing animation.</param>
        void StartTypingAnimation(Text chatText, string stringResult)
        {
            typingTimer -= Time.deltaTime;
            if (typingTimer <= 0)
            {
                if (chatText.text != stringResult || chatText.text.Length < stringResult.Length)
                {
                    chatText.text += stringResult[chatText.text.Length];
                    typingTimer = EcCutsceneManager.instance.chatTypingDelay;
                }
                else
                {
                    startTyping = false;
                }
            }
        }

        /// <summary>
        /// Advances to the next cutscene or finishes the sequence if all cutscenes are completed.
        /// </summary>
        public void PlayNextCutscene()
        {
            if (chatText.text == chatTextString)
            {
                InvokePostEvent();
                if (currentID < cutsceneData.Length - 1)
                {
                    currentID += 1;
                    autoplayTime = EcCutsceneManager.instance.autoplayTime;
                    PlayCutscene();
                }
                else
                {
                    Debug.Log("Cutscene finished");
                    EcCutsceneManager.instance.closeCutscenes();
                }
            }
            else
            {
                chatText.text = chatTextString;
                startTyping = false;
                typingTimer = 0;
            }
        }

        /// <summary>
        /// Automatically progresses to the next cutscene based on the timer.
        /// </summary>
        void AutoPlayingCutscene()
        {
            float temp = EcCutsceneManager.instance.autoplayTime;
            if (temp >= 0 && chatText.text == chatTextString)
            {
                //auto play cutscene on
                autoplayTime -= Time.deltaTime;
                if (autoplayTime <= 0)
                {
                    PlayNextCutscene();
                }
            }
        }
    }
}
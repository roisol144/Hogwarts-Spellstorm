﻿/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Meta.WitAi;
using Meta.WitAi.Json;
using UnityEngine;

namespace Oculus.Voice.Demo
{
    public class InteractionHandler : MonoBehaviour
    {
        [SerializeField] private bool showJson;

        [Header("Voice")]
        [SerializeField] private AppVoiceExperience appVoiceExperience;

        [SerializeField] private string textToSend;

        // Whether voice is activated
        public bool IsActive => _active;
        private bool _active = false;

        // Add delegates
        private void OnEnable()
        {
            appVoiceExperience.VoiceEvents.OnRequestCreated.AddListener(OnRequestStarted);
            appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnRequestTranscript);
            appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnRequestTranscript);
            appVoiceExperience.VoiceEvents.OnStartListening.AddListener(OnListenStart);
            appVoiceExperience.VoiceEvents.OnStoppedListening.AddListener(OnListenStop);
            appVoiceExperience.VoiceEvents.OnStoppedListeningDueToDeactivation.AddListener(OnListenForcedStop);
            appVoiceExperience.VoiceEvents.OnStoppedListeningDueToInactivity.AddListener(OnListenForcedStop);
            appVoiceExperience.VoiceEvents.OnResponse.AddListener(OnRequestResponse);
            appVoiceExperience.VoiceEvents.OnError.AddListener(OnRequestError);
        }
        // Remove delegates
        private void OnDisable()
        {
            appVoiceExperience.VoiceEvents.OnRequestCreated.RemoveListener(OnRequestStarted);
            appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnRequestTranscript);
            appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnRequestTranscript);
            appVoiceExperience.VoiceEvents.OnStartListening.RemoveListener(OnListenStart);
            appVoiceExperience.VoiceEvents.OnStoppedListening.RemoveListener(OnListenStop);
            appVoiceExperience.VoiceEvents.OnStoppedListeningDueToDeactivation.RemoveListener(OnListenForcedStop);
            appVoiceExperience.VoiceEvents.OnStoppedListeningDueToInactivity.RemoveListener(OnListenForcedStop);
            appVoiceExperience.VoiceEvents.OnResponse.RemoveListener(OnRequestResponse);
            appVoiceExperience.VoiceEvents.OnError.RemoveListener(OnRequestError);
        }

        /// Context menu to test voice command
        [ContextMenu("Test voice command")]
        public void TestVoiceCommand()
        {
            appVoiceExperience.Activate(textToSend);
            _active = true;
        }

        // Request began
        private void OnRequestStarted(WitRequest r)
        {
            // Store json on completion
            if (showJson) r.onRawResponse = (response) => Debug.Log("Interaction handler request started " + response);
            // Begin
            _active = true;
        }
        // Request transcript
        private void OnRequestTranscript(string transcript)
        {
            Debug.Log(transcript);
        }
        // Listen start
        private void OnListenStart()
        {
            Debug.Log("Listening...");
        }
        // Listen stop
        private void OnListenStop()
        {
            Debug.Log("Processing...");
        }
        // Listen stop
        private void OnListenForcedStop()
        {
            OnRequestComplete();
        }
        // Request response
        private void OnRequestResponse(WitResponseNode response)
        {
            if (!showJson)
            {
                if (!string.IsNullOrEmpty(response["text"]))
                {
                    Debug.Log("I heard: " + response["text"]);
                }
            }
            OnRequestComplete();
        }
        // Request error
        private void OnRequestError(string error, string message)
        {
            if (!showJson)
            {
                Debug.Log($"<color=\"red\">Error: {error}\n\n{message}</color>");
            }
            OnRequestComplete();
        }
        // Deactivate
        private void OnRequestComplete()
        {
            _active = false;
        }

        // Toggle activation
        public void ToggleActivation()
        {
            SetActivation(!_active);
        }
        // Set activation
        public void SetActivation(bool toActivated)
        {
            if (_active != toActivated)
            {
                _active = toActivated;
                if (_active)
                {
                    appVoiceExperience.Activate();
                }
                else
                {
                    appVoiceExperience.Deactivate();
                }
            }
        }
    }
}

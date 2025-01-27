using UnityEngine;
using UnityEngine.EventSystems;

public class ToggleMusic : MonoBehaviour {
    public GameObject _onImage;
    public GameObject _offImage;
    
    public AudioSource _audioSource;
    
    public void Toggle() {
        if (_audioSource.isPlaying) {
            _audioSource.Stop();
            if (_onImage) _onImage.SetActive(false);
            if (_offImage) _offImage.SetActive(true);
        } else {
            _audioSource.Play();
            if (_onImage) _onImage.SetActive(true);
            if (_offImage) _offImage.SetActive(false);
        }
        
        EventSystem.current.SetSelectedGameObject(null);
    }
}
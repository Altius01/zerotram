﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimeToStopBus : MonoBehaviour
{
	[SerializeField]
	private Image Num_Left;
	[SerializeField]
	private Image Num_Left_Middle;
	[SerializeField]
	private Image Num_Right_Middle;
	[SerializeField]
	private Image Num_Right;
	[SerializeField]
	private DoorsTimer _timer;
	[SerializeField]
	private Text _text;
	private bool _bar_left;
	private bool _bar_left_middle;
	private bool _bar_right_middle;
	private bool _bar_right;
	private int _time;
	private int _time_old;
	private float _num_posY;
	private const float _baraban_speed = 25f;
	private const float _baraban_y = 48f;

	private const int FullZeroCount = 4;

	void Start()
	{
		_num_posY = Num_Left.transform.localPosition.y;
		_timer = GameObject.Find("DoorsTimer").GetComponent<DoorsTimer>();
	}

	void Update()
	{
		int remainTime = _timer.GetCurrentRemainingTime();
		int zeroCount = FullZeroCount - remainTime.ToString().Length;
		string result = string.Empty;
		for (int i = 0; i < zeroCount; i++)
		{
			result += "0";
		}
		result += remainTime;
		// result = result.Insert(2, ":");
		_text.text = result;
		if (_time != int.Parse(result))
		{
			_time = int.Parse(result);
			_bar_left = true;
			_bar_left_middle = true;
			_bar_right_middle = true;
			_bar_right = true;
		}
		int xxxx = Mathf.FloorToInt(_time / 1000);
		int xxx = Mathf.FloorToInt((_time - (xxxx * 1000)) / 100);
		int xx = Mathf.FloorToInt((_time - (xxxx * 1000) - (xxx * 100)) / 10);
		int x = _time - (xxxx * 1000) - (xxx * 100) - (xx * 10);
		if (!_bar_left && !_bar_left_middle && !_bar_right_middle && !_bar_right)
		{
			_time_old = _time;
		}
		else
		{
			if (_bar_left)
			{
				if (Num_Left.transform.localPosition.y > (_num_posY - (_baraban_y * xxxx)))
					Num_Left.transform.Translate(0, Time.deltaTime * -_baraban_speed, 0);
				else
				{
					Num_Left.transform.localPosition = new Vector3(Num_Left.transform.localPosition.x, _num_posY - (_baraban_y * xxxx), Num_Left.transform.localPosition.z);
					_bar_left = false;
				}
			}
			if (_bar_left_middle)
			{
				if (_time < _time_old)
				{
					if (xxx < 9)
					{
						if (Num_Left_Middle.transform.localPosition.y < (_num_posY - (_baraban_y * xxx)))
							Num_Left_Middle.transform.Translate(0, 0.1f * _baraban_speed * (_time_old - _time), 0);
						else
						{
							Num_Left_Middle.transform.localPosition = new Vector3(Num_Left_Middle.transform.localPosition.x, _num_posY - (_baraban_y * xxx), Num_Left_Middle.transform.localPosition.z);
							_bar_left_middle = false;
						}
					}
					else
					{
						if (Num_Left_Middle.transform.localPosition.y > (_num_posY - (_baraban_y * 9)))
							Num_Left_Middle.transform.Translate(0, -0.2f * _baraban_speed, 0);
						else
						{
							Num_Left_Middle.transform.localPosition = new Vector3(Num_Left_Middle.transform.localPosition.x, _num_posY - (_baraban_y * 9), Num_Left_Middle.transform.localPosition.z);
							_bar_left_middle = false;
						}
					}
				}
				else
				{
					if (xxx > 0)
					{
						if (Num_Left_Middle.transform.localPosition.y > (_num_posY - (_baraban_y * xxx)))
							Num_Left_Middle.transform.Translate(0, 0.1f * _baraban_speed * (_time_old - _time), 0);
						else
						{
							Num_Left_Middle.transform.localPosition = new Vector3(Num_Left_Middle.transform.localPosition.x, _num_posY - (_baraban_y * xxx), Num_Left_Middle.transform.localPosition.z);
							_bar_left_middle = false;
						}
					}
					else
					{
						if (Num_Left_Middle.transform.localPosition.y < _num_posY)
							Num_Left_Middle.transform.Translate(0, 0.45f * _baraban_speed, 0);
						else
						{
							Num_Left_Middle.transform.localPosition = new Vector3(Num_Left_Middle.transform.localPosition.x, _num_posY, Num_Left_Middle.transform.localPosition.z);
							_bar_left_middle = false;
						}
					}
				}
			}
			if (_bar_right_middle)
			{
				if (_time < _time_old)
				{
					if (xx < 5)
					{
						if (Num_Right_Middle.transform.localPosition.y < (_num_posY - (_baraban_y * xx)))
							Num_Right_Middle.transform.Translate(0, 0.1f * _baraban_speed * (_time_old - _time), 0);
						else
						{
							Num_Right_Middle.transform.localPosition = new Vector3(Num_Right_Middle.transform.localPosition.x, _num_posY - (_baraban_y * xx), Num_Right_Middle.transform.localPosition.z);
							_bar_right_middle = false;
						}
					}
					else
					{
						if (Num_Right_Middle.transform.localPosition.y > (_num_posY - (_baraban_y * 5)))
							Num_Right_Middle.transform.Translate(0, -0.1f * _baraban_speed, 0);
						else
						{
							Num_Right_Middle.transform.localPosition = new Vector3(Num_Right_Middle.transform.localPosition.x, _num_posY - (_baraban_y * 5), Num_Right_Middle.transform.localPosition.z);
							_bar_right_middle = false;
						}
					}
				}
				else
				{
                    if (xx > 0)
                    {
                        if (Num_Right_Middle.transform.localPosition.y > (_num_posY - (_baraban_y * xx)))
                            Num_Right_Middle.transform.Translate(0, 0.1f * _baraban_speed * (_time_old - _time), 0);
                        else
                        {
                            Num_Right_Middle.transform.localPosition = new Vector3(Num_Right_Middle.transform.localPosition.x, _num_posY - (_baraban_y * xx), Num_Right_Middle.transform.localPosition.z);
                            _bar_right_middle = false;
                        }
                    }
                    else
                    {
                        if (Num_Right_Middle.transform.localPosition.y < _num_posY)
                            Num_Right_Middle.transform.Translate(0, 0.45f * _baraban_speed, 0);
                        else
                        {
                            Num_Right_Middle.transform.localPosition = new Vector3(Num_Right_Middle.transform.localPosition.x, _num_posY, Num_Right_Middle.transform.localPosition.z);
                            _bar_right_middle = false;
                        }
                    }
				}
			}
			if (_time < _time_old)
			{
				if (x < 9)
				{
					if (Num_Right.transform.localPosition.y < (_num_posY - (_baraban_y * x)))
						Num_Right.transform.Translate(0, 0.1f * _baraban_speed * (_time_old - _time), 0);
					else
					{
						Num_Right.transform.localPosition = new Vector3(Num_Right.transform.localPosition.x, _num_posY - (_baraban_y * x), Num_Right.transform.localPosition.z);
						_bar_right = false;
					}
				}
				else
				{
					if (Num_Right.transform.localPosition.y > (_num_posY - (_baraban_y * 9)))
						Num_Right.transform.Translate(0, -0.2f * _baraban_speed, 0);
					else
					{
						Num_Right.transform.localPosition = new Vector3(Num_Right.transform.localPosition.x, _num_posY - (_baraban_y * 9), Num_Right.transform.localPosition.z);
						_bar_right = false;
					}
				}
			}
			else
			{
				if (x > 0)
				{
					if (Num_Right.transform.localPosition.y > (_num_posY - (_baraban_y * x)))
						Num_Right.transform.Translate(0, 0.1f * _baraban_speed * (_time_old - _time), 0);
					else
					{
						Num_Right.transform.localPosition = new Vector3(Num_Right.transform.localPosition.x, _num_posY - (_baraban_y * x), Num_Right.transform.localPosition.z);
						_bar_right = false;
					}
				}
				else
				{
					if (Num_Right.transform.localPosition.y < _num_posY)
						Num_Right.transform.Translate(0, 0.45f * _baraban_speed, 0);
					else
					{
						Num_Right.transform.localPosition = new Vector3(Num_Right.transform.localPosition.x, _num_posY, Num_Right.transform.localPosition.z);
						_bar_right = false;
					}
				}
			}
		}
	}

}

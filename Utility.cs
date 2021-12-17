using System.Collections;
using System.Collections.Generic;

public static class Utility // 내부 메서드에 쉽게 접근할 수 있도록 static 사용
{
  public static T[] ShuffleArray<T>(T[] array, int seed) 
    {
        System.Random prng = new System.Random(seed); // prng = 가짜 랜덤 숫자 생성기

        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length); // 최대값과 최소값
            T tempItem = array[randomIndex]; // 무작위로 선택한 인덱스를 tempItem에 임시 저장
            array[randomIndex] = array[i]; // randomIndex를 i번 반복 
            array[i] = tempItem; 

        }
        return array; // 셔플한 배열을 반환 
    }
}

using System.Collections.Generic;
using UnityEngine;
using Config;

public class Script_Tile {

	private float _health;
	private float _maxHealth;
	private GameObject _tileObject;
	private Color _colorCurrent;
	private Color _colorDirt;
	private Color _colorGrass;
	private Color _colorDecayingGrass;

	private bool _isMature;
	private bool _isOld;
	private float _matureTimerCurrent;
	private float _matureTimerMax;

	private delegate void ActDelegate();

	private int _gridXCoordinate;
	private int _gridZCoordinate;

	private bool _isTrampledUpon;

	private Script_Grid _grid;

	private List<Script_Sheep> _nearbySheep;

	private bool _isOccupiedBySheep;
	private bool _isOccupiedByWolf;

	private bool _isEatenUpon;

	private int _sheepSensingRange;

	ActDelegate _actDelegate;

	private GrassStates _state;
	private Vector3Int _position;

	private Material _material;

	public Script_Tile(Script_Grid p_grid, GrassStates p_state, int p_gridXCoordinate,int p_y, int p_gridZCoordinate, Quaternion p_rotation)
	{
		_position = new Vector3Int (p_gridXCoordinate, -1, p_gridZCoordinate);
		_tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_tileObject.name = "TileObject";
		_tileObject.transform.position = _position;
		_tileObject.transform.rotation = p_rotation;
		_material = _tileObject.GetComponent<Renderer>().material;
		_material.color = Color.white;



		_nearbySheep = new List<Script_Sheep> ();
		_isOccupiedBySheep = false;
		_isOccupiedByWolf = false;
		_grid = p_grid;
		_gridXCoordinate = p_gridXCoordinate;
		_gridZCoordinate = p_gridZCoordinate;

		_sheepSensingRange = 2;

		_isOld = false;
		_actDelegate = null;
		_matureTimerMax = 5.0f;
		_matureTimerCurrent = 0.0f;
		_isMature = false;	
		_maxHealth = 10.0f;
		_state = p_state;

		_colorDirt = new Color (1,1,0,1);
		_colorGrass = new Color(0,1,1,1);
		_colorDecayingGrass = new Color (0.5f, 0.5f, 0, 1);

		_isTrampledUpon = false;
		_isEatenUpon = false;

		if (_state == GrassStates.Dirt) {
			_health = 0.0f;
			_colorCurrent = _colorDirt;
		} else if (_state == GrassStates.Grass) {
			_health = 8.0f;
			_colorCurrent = Color.Lerp (_colorDecayingGrass, _colorGrass, _health / _maxHealth);
		}

		SetColor (_colorCurrent);
	}

	public void InitializeGrass()
	{
		if (_state == GrassStates.Grass) {
			float spawnedGrassHealth = 2.0f;

			_actDelegate = null;
			_isEatenUpon = false;
			_isTrampledUpon = false;
			_isOld = false;
			_matureTimerCurrent = 0.0f;
			_isMature = false;	
			_health = spawnedGrassHealth;
			_colorCurrent = Color.Lerp (_colorDecayingGrass, _colorGrass, _health / _maxHealth);
			SetColor (_colorCurrent);
		}
	}

	public Vector3Int GetPositionAsInt()
	{

		int x = (int) _tileObject.transform.position.x;
		int y = 0;
		int z = (int) _tileObject.transform.position.z;

		Vector3Int location = new Vector3Int (x, y, z);

		return location;
	}

	public Vector3 GetPosition()
	{
		float x = _tileObject.transform.position.x;
		float y = 0;
		float z = _tileObject.transform.position.z;

		Vector3 location = new Vector3 (x, y, z);
		return location;
	}

	public void SetOccupiedBySheep(bool p_occupation)
	{
		_isOccupiedBySheep = p_occupation;
	}

	public bool GetOccupiedBySheep()
	{
		return _isOccupiedBySheep;
	}

	public void SetOccupiedByWolf(bool p_occupation)
	{
		_isOccupiedByWolf = p_occupation;
	}

	public bool GetOccupiedByWolf()
	{
		return _isOccupiedByWolf;
	}


		
	public GrassStates GetState()
	{
		return _state;
	}

	public void SetState(GrassStates p_state)
	{
		_state = p_state;
	}


	public List<Script_Tile> GetTilesWithinRange(int p_range)
	{

		List<Script_Tile> tilesWithinRange = new List<Script_Tile>();
		int xCurrent = _gridXCoordinate;
		int zCurrent = _gridZCoordinate;


		int xMin = xCurrent;
		int xMax = xCurrent;
		int zMin = zCurrent;
		int zMax = zCurrent;

		for (int x = xCurrent - p_range; x <= xCurrent; x++) {
			if (x < 0)
				continue;

			xMin = x;
			break;
		}
		for (int x = xCurrent + p_range; x >= xCurrent; x--) {
			if (x >= _grid.GetWidth ())
				continue;

			xMax = x;
			break;
		}
		for (int z = zCurrent - p_range; z <= zCurrent; z++) {
			if (z < 0)
				continue;

			zMin = z;
			break;
		}
		for (int z = zCurrent + p_range; z >= zCurrent; z--) {
			if (z >= _grid.GetHeight ())
				continue;

			zMax = z;
			break;
		}

		for (int z = zMin; z <= zMax; z++) {
			for (int x = xMin; x <= xMax; x++) {
				Script_Tile tile = _grid.AccessGridTile (x, z);
				tilesWithinRange.Add (tile);
			}
		}

		return tilesWithinRange;


	}

	public void SetHealth(float p_health)
	{
		_health = p_health;
	}



	public void SpawnGrass(List<Script_Tile> p_tiles)
	{

		foreach (Script_Tile tile in p_tiles)
		{
			int grassSpawningNumberPool = Random.Range (0, 1000);
			int numberToSpawnGrass = 999;

			float healthLoss = _health * 0.5f;


			if (grassSpawningNumberPool == numberToSpawnGrass) {

				if (tile.GetState () == GrassStates.Dirt) {

					tile.SetColor(_colorDirt);
					tile.SetState (GrassStates.Grass);
					tile.InitializeGrass ();
					_health = healthLoss;
					_colorCurrent = Color.Lerp (_colorDecayingGrass, _colorGrass, _health / _maxHealth);
				}
			}
		}
	}

	public void SetColor(Color p_color)
	{
		_material.color = p_color;
	}
		

	private List<Script_Sheep> SenseNearbySheep(int p_range)
	{
		List<Script_Sheep> allSheep = _grid.GetSheep ();
		List<Script_Sheep> SheepWithinRange = new List<Script_Sheep>();
		int xCurrent = GetPositionAsInt().x;
		int zCurrent = GetPositionAsInt().z;


		int xMin = xCurrent;
		int xMax = xCurrent;
		int zMin = zCurrent;
		int zMax = zCurrent;

		for (int x = xCurrent - p_range; x <= xCurrent; x++) {
			if (x < 0)
				continue;

			xMin = x;
			break;
		}
		for (int x = xCurrent + p_range; x >= xCurrent; x--) {
			if (x >= _grid.GetWidth ())
				continue;

			xMax = x;
			break;
		}
		for (int z = zCurrent - p_range; z <= zCurrent; z++) {
			if (z < 0)
				continue;

			zMin = z;
			break;
		}
		for (int z = zCurrent + p_range; z >= zCurrent; z--) {
			if (z >= _grid.GetHeight ())
				continue;

			zMax = z;
			break;
		}

		for (int z = zMin; z <= zMax; z++) {
			for (int x = xMin; x <= xMax; x++) {
				foreach (Script_Sheep sheep in allSheep)
				{
					int sheepX = sheep.GetCurrentGridPosition ().x;
					int sheepZ = sheep.GetCurrentGridPosition ().z;						
						if (sheepX >= xMin && sheepX <= xMax
						&& sheepZ >= zMin && sheepZ <= zMax)
						{
							SheepWithinRange.Add (sheep);
						}												
				}
			}
		}

		if (SheepWithinRange.Count > 0)
			return SheepWithinRange;


			return null;
	}


	public void Sense()
	{

		if (_state == GrassStates.Grass) {


			_nearbySheep = SenseNearbySheep(_sheepSensingRange);

			_isTrampledUpon = CollidingWithSheep ();
			_isEatenUpon = EatenBySheep ();

			DetermineMaturity ();


			_isOld = DetermineIfOld ();

		}
	}


	private void DetermineMaturity()
	{
		if (_health >= _maxHealth)
			_isMature = true;
	}


	private bool CollidingWithSheep()
	{
		if (_nearbySheep != null) {
			int x = GetPositionAsInt ().x;
			int z = GetPositionAsInt ().z;

			foreach (Script_Sheep sheep in _nearbySheep) {
				int sheepX = sheep.GetCurrentGridPosition ().x;
				int sheepZ = sheep.GetCurrentGridPosition ().z;

				if (sheepX == x && sheepZ == z) {
					return true;
				}

			}
		}
		return false;
	}

	private bool EatenBySheep()
	{
		if (_nearbySheep != null) {
			if (CollidingWithSheep ()) {
				foreach (Script_Sheep sheep in _nearbySheep) {
					if (sheep.GetGrassBeingEaten () != null) {
						if (sheep.GetGrassBeingEaten () == this) {
							return true;
						}
					}
				}

			}
		}

		return false;
	}

	public void Decide()
	{
		if (_state == GrassStates.Grass) {
			
			if (!_isEatenUpon && _isMature && !_isOld)
				_actDelegate += new ActDelegate (Reproduction);
			
			if (!_isEatenUpon && !_isTrampledUpon && (!_isMature || _isOld)) {				
				_actDelegate = new ActDelegate (Growth);
			}
			if (_health <= 0.0f) {
				_actDelegate = new ActDelegate (Die);
			}
				

		}

	}

	private void Reproduction()
	{
			TryReproduction();
	}

	private void TryReproduction()
	{
		int reproductionRange = 1;
		List<Script_Tile> tiles = GetTilesWithinRange (reproductionRange);
		SpawnGrass (tiles);

	}



	private void Growth()
	{
		if (!_isMature && !_isOld) { 
			Grow();
		}
			
		if (_isOld) {
			Shrink();
		}
	}

	void Grow()
	{
		float growthSpeed = 0.1f;

		_health += growthSpeed;
		_health = Mathf.Clamp (_health, 0, _maxHealth);
		_colorCurrent = Color.Lerp (_colorDecayingGrass, _colorGrass, _health / _maxHealth);

	}

	void Shrink()
	{
		float shrinkSpeed = 0.1f;

		if (_health > 0.0f)
			_health -= shrinkSpeed;

		_health = Mathf.Clamp (_health, 0, _maxHealth);
		_colorCurrent = Color.Lerp (_colorDecayingGrass, _colorGrass, _health / _maxHealth);
	}

	void Die()
	{
		_state = GrassStates.Dirt;
		_colorCurrent = _colorDirt;

		_isOld = false;
		_isMature = false;
		_isTrampledUpon = false;
		_isEatenUpon = false;
	}

	public void Act()
	{
		if (_state == GrassStates.Grass) {
			if (_actDelegate != null)
				_actDelegate ();

			SetColor (_colorCurrent);
		}
	}

	private bool DetermineIfOld()
	{
		float maturityTimerSpeed = 1.0f;

		if (_isMature) {
			if (_matureTimerCurrent < _matureTimerMax) {
				_matureTimerCurrent += maturityTimerSpeed;
				return false;
			}
			return true;
		}

		return false;
	}


	public void DecreaseHealth(float p_amount)
	{
		_health -= p_amount;
		_health = Mathf.Clamp (_health, 0, _maxHealth);
		_colorCurrent = Color.Lerp (_colorDecayingGrass, _colorGrass, _health / _maxHealth);
	}

	public void NourishGrass()
	{
		float nourishmentHealth = 5.0f;

		if (_state == GrassStates.Dirt) {
			SetColor(_colorDirt);
			SetState (GrassStates.Grass);
			InitializeGrass ();
			SetHealth (nourishmentHealth);
		} else {
			_health += nourishmentHealth;
			_health = Mathf.Clamp (_health, 0, _maxHealth);
			_colorCurrent = Color.Lerp (_colorDecayingGrass, _colorGrass, _health / _maxHealth);
		}
	}

}